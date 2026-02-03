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

            //Save items inserted
            foreach (ItemDTO item in configuration.items)
            {
                if (item.Id == 0)
                {
                    return Problem("Could not save Item due to Id being 0");
                }

                //Save item
                itemContext.Items.Add(item);

                //Configuration item lists
                configurationItemListsContext.ConfigurationItemLists
                    .Add(new ConfigurationItemLists() { ConfigurationId = configuration.Id, ItemId = item.Id });
            }

            await itemContext.SaveChangesAsync();
            await configurationItemListsContext.SaveChangesAsync();

            //Save category
            categoryContext.Categories.Add(configuration.category);
            await categoryContext.SaveChangesAsync();

            //Save co-ordinate
            coOrdinateContext.CoOrdinates.Add(configuration.coOrd);
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

            return CreatedAtAction(
                actionName: "PostConfiguration",
                routeValues: new { id = configuration.Id },
                value: configuration);
        }

        [HttpDelete]
        [Route("/configurations/{id}")]
        public async Task<ActionResult> DeleteConfiguration(int id)
        {
            //Delete configuration
            if (await configurationContext.Configurations.FindAsync(id) is Configuration configuration)
            {
                configurationContext.Configurations.Remove(configuration);
                await configurationContext.SaveChangesAsync();
            }
            else
            {
                return NotFound();
            }

            //Delete ConfigurationItemLists
            List<ConfigurationItemLists> configurationItemLists = await configurationItemListsContext.ConfigurationItemLists.Where(configItemList => configItemList.ConfigurationId == id).ToListAsync();
            foreach(ConfigurationItemLists configItemList in configurationItemLists)
            {
                configurationItemListsContext.Remove(configItemList);
                await configurationItemListsContext.SaveChangesAsync();
            }

            return NoContent();
        }
    }
}
