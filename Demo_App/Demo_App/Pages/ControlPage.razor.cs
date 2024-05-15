using AssociationBusiness.Handlers;
using AssociationBusiness.Handlers.Components;
using AssociationBusiness.Handlers.Rows;
using AssociationEntities.Models.Api;
using Azure.Core;
using Demo_App.Data;
using Demo_App.Models;
using Demo_App.Services;
using Demo_App.Services.Blogging;
using Demo_App.Services.Events;
using Demo_App.Services.Pages;
using MediatR.NotificationPublishers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using Syncfusion.Blazor.RichTextEditor;
using Syncfusion.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.ObjectiveC;

namespace Demo_App.Pages
{
    public partial class ControlPage : ComponentBase
    {
        // Injected services
        [Inject]
        public ICustomMenuService customMenuService { get; set; }

        [Inject]
        public IPageService pageService { get; set; }

        [Inject]

        public IRowService rowService { get; set; }
        [Inject]
        public IContainerService containerService { get; set; }

        [Inject]
        public IComponentService componentService { get; set; }
        [Inject]

        public IComponentPropertyService componentPropertyService { get; set; }
        [Inject]
        public IEventService eventService { get; set; }

        [Inject]
        public IBlogPageService blogPageService { get; set; }

        [Inject]
        private NavigationManager Navigation { get; set; }


        // Parameters
        [Parameter] public string TenantId { get; set; }
        [Parameter] public string PageId { get; set; }

        // Private fields
        private int tenantId = 0;
        private int pageId = 0;
        private bool isEdit = false;
        private bool isPageCreated = false;
        private IEnumerable<Models.CustomMenuModel> Menus = new List<Models.CustomMenuModel>();
        private List<Models.CustomMenuModel> DropdownList = new List<Models.CustomMenuModel>();
        private Models.PageModel formData = new Models.PageModel { MenuName = "Please Select" }; // Initialize with default menu name
        private List<string> BlogTagSuggestions = new List<string>();
        private bool isMenuMapped = false;
        private string selectedMenuName { get; set; }
        private PageModel existedPage = new PageModel();
        private bool isContentVisible = false;
        private bool isDialogVisible = false;
        private bool isResourcePathExists = true;
        private String base64Image = "";
        private String HTMLContent = "";
        private IEnumerable<Models.RowModel> rowsA = new List<Models.RowModel>();
        private List<String> EventTypes = new List<String>();
        private string searchEventTerm = "";
        private string searchBlogTerm = "";
        private string searchTerm = "";
        private List<EnumItem> EnumItemList { get; set; }
        private List<EventModel> Events = new List<EventModel>();
        private List<BlogPagesModel> BlogList = new List<BlogPagesModel>();
        private Component currentComponent { get; set; } //= new Component();
        private IEnumerable<EventModel> filteredEvents => Events.Where(item => item.EventName.Contains(searchEventTerm, StringComparison.OrdinalIgnoreCase));
        private IEnumerable<Contact> filteredItems => Data.Where(item => item.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        private IEnumerable<BlogPagesModel> filteredBlogs => BlogList.Where(item => item.Title.Contains(searchBlogTerm, StringComparison.OrdinalIgnoreCase));
        private List<AssociationEntities.CustomModels.BlogFilterSuggestions> blogFilterSuggestions = new List<AssociationEntities.CustomModels.BlogFilterSuggestions>();

        private List<string> BlogTags = new List<string>();
        private List<string> BlogDiscipline = new List<string>();
        private List<string> BlogDivision = new List<string>();

        private List<string> SelectedBlogTags = new List<string>();
        private List<string> SelectedBlogDiscipline = new List<string>();
        private List<string> SelectedBlogDivision = new List<string>();

        // EnumItem class
        public class EnumItem
        {
            public ComponentType EnumValue { get; set; }
            public string DisplayName { get; set; }
        }

        /// <summary>
        /// Represents a list of toolbar items to Rich Text Editor
        /// </summary>

        private List<ToolbarItemModel> Tools = new List<ToolbarItemModel>()
        {
            new ToolbarItemModel() { Command = ToolbarCommand.Bold },
            new ToolbarItemModel() { Command = ToolbarCommand.Italic },
            new ToolbarItemModel() { Command = ToolbarCommand.Underline },
            new ToolbarItemModel() { Command = ToolbarCommand.Separator },
            new ToolbarItemModel() { Command = ToolbarCommand.Formats },
            new ToolbarItemModel() { Command = ToolbarCommand.Alignments },
            new ToolbarItemModel() { Command = ToolbarCommand.Separator },
            new ToolbarItemModel() { Command = ToolbarCommand.CreateLink },
            new ToolbarItemModel() { Command = ToolbarCommand.Image },
            new ToolbarItemModel() { Command = ToolbarCommand.CreateTable },
            new ToolbarItemModel() { Command = ToolbarCommand.Separator },
            new ToolbarItemModel() { Command = ToolbarCommand.SourceCode },
            new ToolbarItemModel() { Command = ToolbarCommand.Separator },
            new ToolbarItemModel() { Command = ToolbarCommand.Undo },
            new ToolbarItemModel() { Command = ToolbarCommand.Redo }
        };



        /// <summary>
        /// On Initialized Method
        /// </summary>
        /// <returns></returns>
        protected override async Task OnInitializedAsync()
        {
            tenantId = !string.IsNullOrEmpty(TenantId) ? Convert.ToInt32(TenantId) : 0;
            pageId = !string.IsNullOrEmpty(PageId) ? Convert.ToInt32(PageId) : 0;
            if (pageId != 0)
            {
                formData = await pageService.GetPageById(pageId);
                isPageCreated = true;
            }

            await FetchCustomMenu();
            Console.WriteLine("Let's Come work");
            EnumItemList = Enum.GetValues(typeof(ComponentType))
                         .Cast<ComponentType>()
                         .Select(e => new EnumItem { EnumValue = e, DisplayName = e.ToString() })
                         .ToList();

            await FetchRows();

            await GetEventList();
            EventTypes.AddRange(Events.Select(x => x.EventType).Distinct());

            await GetBlogsList();
            BlogTagSuggestions.RemoveAll(s => string.IsNullOrEmpty(s));

            FetchBlogSuggestions();
        }

        // Handles the submission of form data
        private async Task HandleValidSubmit()
        {
            // Check if the resource path already exists
            //isResourcePathExists = await pageService.CheckIfResourcePathAvailable(tenantId, formData.ResourcePath, pageId);
            //if (!isResourcePathExists)
            //{
            //    return;
            //}

            if (formData.MenuId > 0)
            {
                // Check if a page already exists for the selected menu
                existedPage = await pageService.CheckIfPageMapped(formData.MenuId, formData.Id);
            }

            // Check if the page scope type is public and If a page exists, set the selected menu name and flag indicating menu mapping
            if (formData.PageScopeType == "public" && existedPage.Id != 0)
            {
                selectedMenuName = DropdownList.FirstOrDefault(x => x.MenuId == formData.MenuId).MenuName;
                isMenuMapped = true;
                return;
            }

            else
            {
                // If the page scope type is not public, create a new page
                CreatePage();
            }
        }

        // Creates a new page
        private async void CreatePage()
        {
            // If the menu is already mapped, update the existing page
            if (isMenuMapped)
            {
                // Copy data from existing page to update request model
                CreatePageRequest updateMenuRequest = new CreatePageRequest()
                {
                    PageTitle = existedPage.PageTitle,
                    IsHomePage = existedPage.IsHomePage,
                    IsLandingPage = existedPage.IsLandingPage,
                    ResourcePath = existedPage.ResourcePath,
                    PageScopeType = "draft",
                    MenuId = existedPage.MenuId,
                    TenantId = tenantId,
                    Id = existedPage.Id,
                    pageUrl = DropdownList.Where(x => x.MenuId == formData.MenuId).Select(x => x.MenuName).FirstOrDefault() + "/" + existedPage.ResourcePath ?? ""
                };

                // Create or update the page
                var updatePageId = await pageService.CreatePage(updateMenuRequest);
                existedPage = new(); // Reset existedPage
            }

            // Copy data from form to create request model
            CreatePageRequest createMenuRequest = new CreatePageRequest()
            {
                PageTitle = formData.PageTitle,
                IsHomePage = formData.IsHomePage,
                ResourcePath = formData.ResourcePath,
                PageScopeType = formData.PageScopeType,
                IsLandingPage = formData.IsLandingPage,
                MenuId = formData.MenuId,
                TenantId = tenantId,
                Id = pageId,
                pageUrl = DropdownList.Where(x => x.MenuId == formData.MenuId).Select(x => x.MenuName).FirstOrDefault().ToString() + "/" + formData.ResourcePath ?? ""
            };

            // Create the page
            var CurrentPageId = await pageService.CreatePage(createMenuRequest);
            formData = new(); // Reset formData


            if (!isPageCreated)
            {
                // Redirect to page list
                Navigation.NavigateTo($"/ControlPage/{tenantId}/{CurrentPageId}");
            }
            else
            {
                Navigation.NavigateTo($"/PageList/{tenantId}");

            }
        }
        // Initializes the component when parameters are set
        protected async override Task OnParametersSetAsync()
        {
            tenantId = !string.IsNullOrEmpty(TenantId) ? Convert.ToInt32(TenantId) : 0;
            pageId = !string.IsNullOrEmpty(PageId) ? Convert.ToInt32(PageId) : 0;
            if (pageId != 0)
            {
                formData = await pageService.GetPageById(pageId);
                isPageCreated = true;
            }



            await FetchRows();

        }

        /// <summary>
        /// Fetches custom menus asynchronously and constructs a dropdown list of menus with hierarchical names.
        /// </summary>
        public async Task FetchCustomMenu()
        {
            Menus = await customMenuService.GetAllMenus(tenantId);
            DropdownList = new List<Models.CustomMenuModel>();
            DropdownList.Add(new Models.CustomMenuModel() { MenuId = 0, MenuName = "Please Select" });
            DropdownList.AddRange(Menus.Where(x => x.ParentMenuId == 0).ToList());
            DropdownList.AddRange(Menus.Where(x => x.ParentMenuId != 0).ToList());
            DropdownList.ForEach(y => y.MenuName = (y.ParentMenuId != 0 ? (Menus.Where(x => x.MenuId == y.ParentMenuId).Select(x => x.MenuName + "/").FirstOrDefault()) : "") + y.MenuName);
        }


        /// <summary>
        /// Retrieves the value of a specified property from the component's properties. If the property does not exist, returns an empt string.
        /// </summary>
        /// <param name="component">The component from which to retrieve the property value.</param>
        /// <param name="key">The key of the property to retrieve.</param>

        string GetValueOrDefault(Component component, string key) =>
            component.componentProperties.FirstOrDefault(x => x.Key == key)?.Value ?? "";

        /// <summary>
        /// Returns the CSS class for defining grid columns based on the number of columns.
        /// </summary>
        /// <param name="numberOfColumns">The number of columns in the grid.</param>
        private string GetGridClass(int numberOfColumns)
        {
            if (numberOfColumns <= 4)
            {
                return numberOfColumns switch
                {
                    1 => " grid-cols-1",
                    2 => " grid-cols-2",
                    3 => " grid-cols-3",
                    4 => " grid-cols-4",
                    _ => "grid-cols-1"
                };
            }
            else
            {
                return "sm:grid-cols-1 md:grid-cols-2 lg:grid-cols-4";
            }
        }

        /// <summary>
        /// Returns the CSS class for defining font sizes based on the number of columns.
        /// </summary>
        /// <param name="columnCount">The number of columns.</param>
        private string GetFontSizeClass(int columnCount)
        {
            switch (columnCount)
            {
                case 1:
                    return "text-base";
                case 2:
                    return "text-lg";
                case 3:
                    return "text-xl";
                case 4:
                    return "text-sm"; // Adjust this for smaller font size
                                      // Add more cases as needed
                default:
                    return "text-base";
            }
        }
        // Sample data
        private List<Contact> Data { get; set; } = new List<Contact>
        {
            new Contact
            {
                Id = 1,
                Name = "John Doe",
                PhoneNumber = "+1234567890",
                EmailAddress = "john@doe.com",
                AvatarImageUrl = "https://res.cloudinary.com/dzax35hss/image/upload/v1707464247/samples/man-on-a-street.jpg"
            },
            new Contact
            {
                Id = 2,
                Name = "Alice Smith",
                PhoneNumber = "+1987654321",
                EmailAddress = "alice@smith.com",
                AvatarImageUrl = "https://res.cloudinary.com/dzax35hss/image/upload/v1707464247/samples/man-on-a-street.jpg"
            },
            new Contact
            {
                Id = 3,
                Name = "Bob Johnson",
                PhoneNumber = "+1122334455",
                EmailAddress = "bob@example.com",
                AvatarImageUrl = "https://res.cloudinary.com/dzax35hss/image/upload/v1707464247/samples/man-on-a-street.jpg"
            },
                        new Contact
            {
                Id = 4,
                Name = "Emily Brown",
                PhoneNumber = "+15556667777",
                EmailAddress = "emily@example.com",
                AvatarImageUrl = "https://res.cloudinary.com/dzax35hss/image/upload/v1707464247/samples/man-on-a-street.jpg"
            },new Contact
            {
                Id = 5,
                Name = "Michael Davis",
                PhoneNumber = "+18889990000",
                EmailAddress = "michael@example.com",
                AvatarImageUrl = "https://res.cloudinary.com/dzax35hss/image/upload/v1707464247/samples/man-on-a-street.jpg"
            }
        };




        //// Define the Contact class
        private class Contact
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string PhoneNumber { get; set; }
            public string EmailAddress { get; set; }
            public string AvatarImageUrl { get; set; }
            //public string Description { get; set; } = "";

        }


        /// <summary>
        /// Updates the details of a page based on the form data.
        /// </summary>
        private async void updatePageDetails()
        {
            UpatePageRequest page = new UpatePageRequest();
            page.Id = pageId;
            page.PageTitle = formData.PageTitle;
            page.ResourcePath = formData.ResourcePath;
            page.PaddingWidth = 3;
            page.IsLandingPage = formData.IsLandingPage;
            page.IsHomePage = formData.IsHomePage;
            page.menuId = formData.MenuId;
            await pageService.UpdatePageInfo(page);
        }

        /// <summary>
        /// Handles the selection of a contact item from a search result.
        /// </summary>
        private void selectItem(Contact contact)
        {
            searchTerm = "";
            currentComponent.RefValue = contact.Id;

        }

        /// <summary>
        /// Fetches a list of events asynchronously.
        /// </summary>
        private async Task GetEventList()
        {
            Events = await eventService.GetAllEvents();

        }

        /// <summary>
        /// Fetches a list of blogs asynchronously.
        /// </summary>
        private async Task GetBlogsList()
        {
            BlogList = await blogPageService.GetAllBlogPages(new AssociationEntities.CustomModels.BlogFilters { TenantId = tenantId });
        }

        /// <summary>
        /// Handles the selection of an event item from a search result.
        /// </summary>
        private void selectItem(EventModel Event, Component component)
        {
            searchEventTerm = "";
            currentComponent.RefValue = Event.EventId;
            currentComponent.ComponentName = Event.EventName;
        }

        /// <summary>
        /// Handles the selection of a blog item from a search result.
        /// </summary>
        private void selectItem(BlogPagesModel blog, Component component)
        {
            searchBlogTerm = "";
            currentComponent.RefValue = blog.BpId;
            currentComponent.ComponentName = blog.Title;
        }

        /// <summary>
        /// Handles the selection of a component type and sets up the component accordingly.
        /// </summary>
        private void selectComponentType(Component component, EnumItem item)
        {
            component.ShowColumnModal = false;
            currentComponent = new Component(component);
            currentComponent.ComponentType = item.DisplayName;

            if (component.IsUpdating && currentComponent.ComponentType != component.ComponentType)
            {
                currentComponent.componentProperties = new List<ComponentPropertyModel>();
                component.IsUpdating = false;
            }



            if (currentComponent.ComponentType == "BlankSpace")
            {
                currentComponent.componentProperties.Add(new ComponentPropertyModel { ComponentId = currentComponent.ComponentId, Key = "BlankSpace", Value = "BlankSpace" });
                ModifyComponent(currentComponent);
            }
            else
            {
                component.ShowPopup = true;
            }

        }

        /// <summary>
        /// Handles the upload of an image file and converts it to base64 format.
        /// </summary>
        private async Task ImageFileUploadHandle(InputFileChangeEventArgs e)
        {
            var image = e.File;
            if (image.ContentType.StartsWith("image/"))
            {
                try
                {

                    var buffer = new byte[image.Size];
                    await image.OpenReadStream().ReadAsync(buffer);
                    base64Image = $"data:{image.ContentType};base64,{Convert.ToBase64String(buffer)}";
                }
                catch (Exception ex)
                {

                }
            }
        }

        /// <summary>
        /// Fetches a list of rows associated with the page asynchronously.
        /// </summary>
        public async Task FetchRows()
        {
            rowsA = await rowService.GetAllRowsByPageId(pageId);
            await InvokeAsync(StateHasChanged);
        }


        public void FetchBlogSuggestions()
        {
            blogFilterSuggestions = blogPageService.GetBlogFilterSuggestions(tenantId);
            if (blogFilterSuggestions != null && blogFilterSuggestions.Any())
            {

                foreach (var item in blogFilterSuggestions)
                {
                    if (item.FilterType == "tag")
                    {
                        BlogTags.Add(item.FilterValue);
                    }
                    else if (item.FilterType == "division")
                    {
                        BlogDivision.Add(item.FilterValue);
                    }
                    else if (item.FilterType == "discipline")
                    {
                        BlogDiscipline.Add(item.FilterValue);
                    }

                }
            }
        }

        /// <summary>
        /// Adds a new row to the page asynchronously.
        /// </summary>
        private async void AddNewRowToPage()
        {

            try
            {
                CreateRowRequest createRowRequest = new CreateRowRequest();
                createRowRequest.PageId = pageId;
                createRowRequest.CreatedOn = DateTime.Now;
                var result = await rowService.CreateRow(createRowRequest);
                await FetchRows();

            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Deletes a row from the page asynchronously.
        /// </summary>
        private async Task DeleteRowInPage(int RowId)
        {
            try
            {
                //await rowService.DeletePageByRowId(RowId);
                await rowService.DeletePageByRowId(RowId);
                await FetchRows();

            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Adds a container to the page with the specified layout.
        /// </summary>
        private async void AddContainer(int RowId, int Layout)
        {
            CreateContainerRequest createContainerRequest = new CreateContainerRequest();
            createContainerRequest.CreatedOn = DateTime.Now;
            createContainerRequest.NoofContainers = Layout;
            createContainerRequest.RowId = RowId;
            try
            {
                var containerId = await containerService.CreateContainer(createContainerRequest);
                CreateComponentsRequest createComponentsRequest = new CreateComponentsRequest();
                List<CreateComponentRequest> ComponentList = new List<CreateComponentRequest>();
                for (int i = 0; i < Layout; i++)
                {
                    CreateComponentRequest component = new CreateComponentRequest { OrderNumber = (i + 1), ContainerId = containerId, CreatedOn = DateTime.Now };
                    ComponentList.Add(component);
                }
                createComponentsRequest.ComponentList = ComponentList;
                await componentService.CreateComponents(createComponentsRequest);
                await FetchRows();

            }
            catch (Exception ex)
            {

            }
        }


        /// <summary>
        /// Modifies a component by updating its properties and saving changes asynchronously.
        /// </summary>
        private async void ModifyComponent(Component component)
        {
            if (component == null)
            {
                return;
            }
            UpdateComponentRequest updateComponentRequest = new UpdateComponentRequest
            {
                ComponentType = component.ComponentType,
                UpdateOn = DateTime.Now,
                ComponentId = component.ComponentId
            };
            CreateComponentPropertiesRequest create = new CreateComponentPropertiesRequest();
            create.PropertyList = new List<CreateComponentPropertyRequest>();

            try
            {


                if (component.componentProperties != null && component.componentProperties.Any())
                {
                    await componentPropertyService.DeleteComponentPropertiesByComponentId(component.ComponentId);
                    foreach (var componentProperty in component.componentProperties)
                    {

                        if (componentProperty != null)
                        {
                            create.PropertyList.Add(new CreateComponentPropertyRequest
                            {
                                ComponentId = componentProperty.ComponentId,
                                Key = componentProperty.Key,
                                Value = componentProperty.Value
                            });
                        }
                    }
                }

                await componentService.UpdateComponent(updateComponentRequest);

                if (create.PropertyList != null && create.PropertyList.Any())
                {

                    var additionalProperties = GetComponentProperties(component);
                    if (additionalProperties != null)
                    {
                        create.PropertyList.AddRange(additionalProperties);
                    }
                }
                await componentPropertyService.CreateComponentProperties(create);
                await FetchRows();
                currentComponent = new Component();
                SelectedBlogTags = new();
                SelectedBlogDiscipline = new();
                SelectedBlogDivision = new();
            }
            catch (Exception ex)
            {

            }

        }

        /// <summary>
        /// Retrieves the properties of a given component.
        /// </summary>
        /// <param name="component">The component for which properties are to be retrieved.</param>
        public List<CreateComponentPropertyRequest> GetComponentProperties(Component component)
        {
            // List to hold component properties
            List<CreateComponentPropertyRequest> componentPropertyModel = new List<CreateComponentPropertyRequest>();

            // Mapping of ComponentType to respective data sources
            Dictionary<string, object> dataSourceMap = new Dictionary<string, object>
    {
        {"ContactPerson", Data},
        {"EventDetail", Events},
        {"BlogDetail", BlogList}
    };

            // Check if the component type exists in the data source map
            if (dataSourceMap.TryGetValue(component.ComponentType, out object dataSource))
            {
                // Object to hold component properties
                object componentProperties = null;

                // Retrieve component properties based on component type
                switch (component.ComponentType)
                {
                    case "ContactPerson":
                        componentProperties = ((IEnumerable<Contact>)dataSource).FirstOrDefault(x => x.Id == component.RefValue);
                        break;
                    case "EventDetail":
                        componentProperties = ((IEnumerable<EventModel>)dataSource).FirstOrDefault(x => x.EventId == component.RefValue);
                        break;
                    //case "BlogDetail":
                    //    componentProperties = ((IEnumerable<BlogPagesModel>)dataSource).FirstOrDefault(x => x.BlogId == component.RefValue);
                    //    break;
                    case "FreeText":
                        componentProperties = new FreeTextModel(component.Description);
                        break;
                    case "BlankSpace":
                        componentProperties = new BlankSpaceModel();
                        break;
                }

                // If component properties are retrieved
                if (componentProperties != null)
                {
                    // Iterate through each property of the component
                    foreach (PropertyInfo obj in componentProperties.GetType().GetProperties())
                    {
                        // Retrieve property value
                        object value = obj.GetValue(componentProperties);

                        // If property value is not null, add to component property model
                        if (value != null)
                        {
                            string key = obj.Name;
                            CreateComponentPropertyRequest c = new CreateComponentPropertyRequest { ComponentId = component.ComponentId, Key = key, Value = value.ToString() };
                            componentPropertyModel.Add(c);
                        }
                    }
                }
            }

            // Return the list of component properties
            return componentPropertyModel;
        }

        /// <summary>
        /// Deletes a component and its associated properties by ID.
        /// </summary>
        /// <param name="Id">The ID of the component to delete.</param>
        private async Task DeleteComponent(int Id)
        {
            try
            {
                // Delete component properties by ID
                await componentPropertyService.DeleteComponentPropertiesByComponentId(Id);

                // Fetch rows to update UI or refresh data
                await FetchRows();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                Console.WriteLine($"Error deleting component: {ex.Message}");
                // Optionally rethrow the exception if you want the calling code to handle it
                // throw;
            }
        }


        private void ComponentSettings(Component component, Container container)
        {
            currentComponent = component;
            component.ShowPopup = true;
            component.ContainerId = container.ContainerId;
            component.IsUpdating = true;
        }


        private BlogPagesModel GetBlogDetailsById(string BlogId)
        {
            BlogPagesModel data = blogPageService.GetBlogPageById(Convert.ToInt32(BlogId));
            if (data == null)
                return new BlogPagesModel();
            return data;
        }


    }


}