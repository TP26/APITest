using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APITest.Models;

namespace APITest.Controllers
{
    [Route("/configurations")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly ConfigurationContext configurationContext;
        private readonly ItemContext itemContext;
        private readonly ConfigurationItemListsContext configurationItemListsContext;
        private readonly CategoryContext categoryContext;
        private readonly CoOrdinatesContext coOrdinateContext;

        public ConfigurationController(ConfigurationContext configurationContextInput, ItemContext itemContextInput,
            ConfigurationItemListsContext configurationItemListsContextInput,
            CategoryContext categoryContextInput, CoOrdinatesContext coOrdinateContextInput)
        {
            configurationContext = configurationContextInput;
            itemContext = itemContextInput;
            configurationItemListsContext = configurationItemListsContextInput;
            categoryContext = categoryContextInput;
            coOrdinateContext = coOrdinateContextInput;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ConfigurationDTO>>> GetConfigurations()
        {
            List<Configuration> configurations = await configurationContext.Configurations.ToListAsync();
            List<ConfigurationDTO> configurationDTOs = new List<ConfigurationDTO>();

            foreach (Configuration config in configurations)
            {
                int configId = config.Id;

                //Get items
                List<ConfigurationItemLists> itemLists = await configurationItemListsContext.ConfigurationItemLists.Where(configurationitemList => configurationitemList.ConfigurationId == configId).ToListAsync();
                List<ItemDTO> items = new List<ItemDTO>();
                List<int> retrievedItems = new List<int>();

                foreach(ConfigurationItemLists itemList in itemLists)
                {
                    int itemId = itemList.ItemId;

                    if (retrievedItems.Contains(itemId) == false)
                    {
                        ItemDTO? item = await itemContext.Items.Where(item => item.Id == itemId).FirstOrDefaultAsync();

                        if (item != null)
                        {
                            items.Add(item);
                        }

                        retrievedItems.Add(itemId);
                    }
                }

                //Get category
                Category? configCategory = await categoryContext.Categories.Where(category => category.Id == config.CategoryId).FirstOrDefaultAsync();

                //Get Co-Ordinate
                CoOrdinates? configCoOrd = await coOrdinateContext.CoOrdinates.Where(coOrd => coOrd.Id == config.CoOrdId).FirstOrDefaultAsync();

                configurationDTOs.Add(new ConfigurationDTO()
                {
                    Id = configId,
                    items = items,
                    category = configCategory,
                    coOrd = configCoOrd
                });
            }

            ClearContexts();

            return Ok(configurationDTOs);
        }

        [HttpPost]
        public async Task<ActionResult<ConfigurationDTO>> PostConfiguration(ConfigurationDTO configuration)
        {
            List<Configuration> existingConfigurations = await configurationContext.Configurations.ToListAsync();
            bool configurationIdUsed = false;

            foreach(Configuration existingConfiguration in existingConfigurations)
            {
                if (existingConfiguration.Id == configuration.Id)
                {
                    configurationIdUsed = true;
                }
            }

            if (configurationIdUsed)
            {
                return Problem("Configuration Id is already in use. Configuration not created.");
            }

            try
            {
                //Save items inserted
                foreach (ItemDTO item in configuration.items)
                {
                    if (item.Id == 0)
                    {
                        return Problem("Could not save Item due to Id being 0");
                    }

                    //Check if item exists
                    ItemDTO? contextItem = await itemContext.Items.FindAsync(item.Id);

                    if (contextItem != null)
                    {
                        //Update item
                        contextItem.Name = item.Name;
                        contextItem.Position = item.Position;
                    }
                    else
                    {
                        //Save item
                        itemContext.Items.Add(item);
                    }

                    //Configuration item lists
                    configurationItemListsContext.ConfigurationItemLists
                        .Add(new ConfigurationItemLists() { ConfigurationId = configuration.Id, ItemId = item.Id });
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error saving configuration items - {ex.Message}");
                return Problem("Error in saving configuration items");
            }

            await itemContext.SaveChangesAsync();
            await configurationItemListsContext.SaveChangesAsync();

            //Save category
            Category? category = await categoryContext.Categories.FindAsync(configuration.category.Id);

            if (category != null)
            {
                category.Name = configuration.category.Name;
            }
            else
            {
                categoryContext.Categories.Add(configuration.category);
            }
                
            await categoryContext.SaveChangesAsync();

            //Save co-ordinate

            CoOrdinates? coOrdinates = await coOrdinateContext.CoOrdinates.FindAsync(configuration.coOrd.Id);

            if (coOrdinates != null)
            {
                coOrdinates.X = configuration.coOrd.X;
                coOrdinates.Y = configuration.coOrd.Y;
            }
            else
            {
                coOrdinateContext.CoOrdinates.Add(configuration.coOrd);
            }

            await coOrdinateContext.SaveChangesAsync();

            //Save configuration
            Configuration dbConfig = new Configuration()
            {
                Id = configuration.Id,
                CategoryId = configuration.category.Id,
                CoOrdId = configuration.coOrd.Id
            };
            configurationContext.Configurations.Add(dbConfig);
            await configurationContext.SaveChangesAsync();

            ClearContexts();

            return CreatedAtAction(
                actionName: "PostConfiguration",
                value: configuration);
        }

        [HttpPut]
        [Route("/configurations/{id}")]
        public async Task<ActionResult<ConfigurationDTO>> PutConfiguration(int id, ConfigurationDTO configuration)
        {
            //Update configuration
            Configuration? configurationRetrieved = await configurationContext.Configurations.FindAsync(id);

            if (configurationRetrieved == null)
            {
                return NotFound();
            }

            configurationRetrieved.CoOrdId = configuration.coOrd.Id;
            configurationRetrieved.CategoryId = configuration.category.Id;
            await configurationContext.SaveChangesAsync();

            //Update category
            Category? category = await categoryContext.Categories.FindAsync(configuration.category.Id);

            if (category == null)
            {
                //Save category
                categoryContext.Categories.Add(configuration.category);
                await categoryContext.SaveChangesAsync();
            }
            else
            {
                //Update category
                category.Name = configuration.category.Name;
                await categoryContext.SaveChangesAsync();
            }

            //Update co ordinates
            CoOrdinates? coOridnate = await coOrdinateContext.CoOrdinates.FindAsync(configuration.coOrd.Id);

            if (coOridnate == null)
            {
                //Save co ordinate
                coOrdinateContext.CoOrdinates.Add(configuration.coOrd);
                await coOrdinateContext.SaveChangesAsync();
            }
            else
            {
                //Update co ordinate
                coOridnate.X = configuration.coOrd.X;
                coOridnate.Y = configuration.coOrd.Y;
                await coOrdinateContext.SaveChangesAsync();
            }

            //Update configuration item links
            //Get existing item links
            List<ConfigurationItemLists> existingConfigurationItemLists = await configurationItemListsContext.ConfigurationItemLists
                .Where(list => list.ConfigurationId == configuration.Id)
                .ToListAsync();
            List<ConfigurationItemLists> configurationItemListsToBeRemoved = new List<ConfigurationItemLists>();

            //Remove existing connections that are no longer present in the configuration
            foreach(ConfigurationItemLists existingList in existingConfigurationItemLists)
            {
                bool configurationExists = false;

                foreach(ItemDTO item in configuration.items)
                {
                    if (item.Id == existingList.ItemId)
                    {
                        configurationExists = true;
                    }
                }

                //If existing connection not included in updated items, delete it
                if (!configurationExists)
                {
                    configurationItemListsContext.ConfigurationItemLists.Remove(existingList);
                }
            }

            await configurationItemListsContext.SaveChangesAsync();

            //Add new lists if they do not exist
            foreach(ItemDTO item in configuration.items)
            {
                ConfigurationItemLists? existingList = null;

                try
                {
                    existingList = await configurationItemListsContext.ConfigurationItemLists
                        .Where(list => list.ConfigurationId == configuration.Id && list.ItemId == item.Id)
                        .FirstAsync();
                }
                catch(Exception ex)
                {
                }

                //Add item list if it does not exist
                if (existingList == null)
                {
                    configurationItemListsContext.ConfigurationItemLists.Add(new ConfigurationItemLists() { ConfigurationId = id, ItemId = item.Id });
                }
            }

            await configurationItemListsContext.SaveChangesAsync();

            //Add new items if they do not exist
            foreach (ItemDTO item in configuration.items)
            {
                ItemDTO? databaseItem = await itemContext.Items.Where(dbItem => dbItem.Id == item.Id).FirstOrDefaultAsync();

                if (databaseItem != null)
                {
                    if (databaseItem.Name != item.Name || databaseItem.Position != item.Position)
                    {
                        databaseItem.Name = item.Name;
                        databaseItem.Position = item.Position;
                    }
                }
                else
                {
                    itemContext.Items.Add(item);
                }

                await itemContext.SaveChangesAsync();
            }

            ClearContexts();

            return NoContent();
        }

        [HttpDelete]
        [Route("/configurations/{id}")]
        public async Task<ActionResult> DeleteConfiguration(int id)
        {
            Console.WriteLine($"Deleting configuration of id {id}");
            //Delete configuration
            if (await configurationContext.Configurations.FindAsync(id) is Configuration configuration)
            {
                Console.WriteLine("Configuration found.");
                configurationContext.Configurations.Remove(configuration);
                await configurationContext.SaveChangesAsync();

                //Remove co ordinates. Counting co-ordinates as a sub part of a configuration, added or removed with it
                CoOrdinates? coOridnates = await coOrdinateContext.CoOrdinates.FindAsync(configuration.CoOrdId);
                if (coOridnates != null)
                {
                    coOrdinateContext.CoOrdinates.Remove(coOridnates);
                    await coOrdinateContext.SaveChangesAsync();
                }
            }
            else
            {
                return NotFound();
            }

            //Delete ConfigurationItemLists
            List<ConfigurationItemLists> configurationItemLists = await configurationItemListsContext.ConfigurationItemLists.Where(configItemList => configItemList.ConfigurationId == id).ToListAsync();
            foreach(ConfigurationItemLists configItemList in configurationItemLists)
            {
                Console.WriteLine($"Removing configuration item list of Id: {configItemList.Id}, item id: {configItemList.ItemId}, config id: {configItemList.ConfigurationId}");
                configurationItemListsContext.ConfigurationItemLists.Remove(configItemList);
                await configurationItemListsContext.SaveChangesAsync();
            }

            ClearContexts();

            return NoContent();
        }
        
        private void ClearContexts()
        {
            configurationContext.ChangeTracker.Clear();
            itemContext.ChangeTracker.Clear();
            configurationItemListsContext.ChangeTracker.Clear();
            categoryContext.ChangeTracker.Clear();
            coOrdinateContext.ChangeTracker.Clear();
        }
    }
}
