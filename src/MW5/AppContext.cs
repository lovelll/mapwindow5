﻿using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using MW5.Api.Interfaces;
using MW5.Api.Legend.Abstract;
using MW5.Controls;
using MW5.Data.Repository;
using MW5.Helpers;
using MW5.Plugins;
using MW5.Plugins.Concrete;
using MW5.Plugins.Enums;
using MW5.Plugins.Events;
using MW5.Plugins.Interfaces;
using MW5.Plugins.Interfaces.Projections;
using MW5.Plugins.Mvp;
using MW5.Plugins.Services;
using MW5.Projections.Helpers;
using MW5.Services.Serialization;
using MW5.Shared;
using MW5.Tools.Toolbox;
using MW5.Tools.Views;
using MW5.UI;
using MW5.UI.Docking;
using MW5.UI.Forms;
using MW5.UI.Menu;
using MW5.UI.Style;
using MW5.Views;

namespace MW5
{
    /// <summary>
    /// Central class storing all the resource avaialable for plugins.
    /// </summary>
    public class AppContext: ISecureContext
    {
        private readonly IApplicationContainer _container;
        private readonly IProjectionDatabase _projectionDatabase;
        private readonly IStyleService _styleService;
        private readonly ITaskCollection _tasks;

        private IMap _map;
        private IMenu _menu;
        private IAppView _view;
        private IMainView _mainView;
        private IProjectService _project;
        private IToolbarCollection _toolbars;
        private IPluginManager _pluginManager;
        private IBroadcasterService _broadcaster;
        private IStatusBar _statusBar;
        private IDockPanelCollection _dockPanelCollection;
        private SynchronizationContext _synchronizationContext;
        private IConfigService _configService;
        private LocatorPresenter _locator;
        private LegendPresenter _legendPresenter;
        private ToolboxPresenter _toolboxPresenter; 
        private IRepository _repository;

        private bool _initialized;

        public AppContext(IApplicationContainer container, IProjectionDatabase projectionDatabase, 
            IStyleService styleService, ITaskCollection tasks)
        {
            Logger.Current.Debug("In AppContext");
            if (container == null) throw new ArgumentNullException("container");
            if (styleService == null) throw new ArgumentNullException("styleService");
            if (tasks == null) throw new ArgumentNullException("tasks");

            _container = container;
            _projectionDatabase = projectionDatabase;
            _styleService = styleService;
            _tasks = tasks;
        }

        /// <summary>
        /// Sets all the necessary references from the main view. 
        /// </summary>
        /// <remarks>We don't use contructor injection here since most of other services use this one as a parameter.
        /// Perhaps property injection can be used.</remarks>
        internal void Init(IMainView mainView, IProjectService project, IConfigService configService, 
                        LegendPresenter legendPresenter, ToolboxPresenter toolboxPresenter, IRepository repository)
        {
            if (mainView == null) throw new ArgumentNullException("mainView");
            if (project == null) throw new ArgumentNullException("project");
            if (legendPresenter == null) throw new ArgumentNullException("legendPresenter");
            if (toolboxPresenter == null) throw new ArgumentNullException("toolboxPresenter");

            _toolboxPresenter = toolboxPresenter;
            _legendPresenter = legendPresenter;
            var legend = _legendPresenter.Legend;
            mainView.Map.Legend = legend;
            legend.Map = mainView.Map;

            // it's expected here that we are on the UI thread
            _synchronizationContext = SynchronizationContext.Current;

            _pluginManager = _container.GetSingleton<IPluginManager>();
            _broadcaster = _container.GetSingleton<IBroadcasterService>();
            _container.RegisterInstance<IMuteMap>(mainView.Map);

            _mainView = mainView;
            _view = new AppView(mainView, _styleService);
            _project = project;
            _map = mainView.Map;
            _configService = configService;
            _repository = repository;

            Legend.Lock();

            _dockPanelCollection = new DockPanelCollection(mainView.DockingManager, mainView as Form, _broadcaster, _styleService);
            _menu = MenuFactory.CreateMainMenu(mainView.MenuManager);
            _toolbars = MenuFactory.CreateMainToolbars(mainView.MenuManager);
            _statusBar = MenuFactory.CreateStatusBar(mainView.StatusBar, PluginIdentity.Default);

            _projectionDatabase.ReadFromExecutablePath(Application.ExecutablePath);

            _repository.Initialize(this);

			// comment this line to prevent locator loading            
			// may be useful for ocx debugging to not create additional 
			// instance of map
			_locator = new LocatorPresenter(_map);  

            this.InitDocking();

            _initialized = true;
        }

        internal void InitPlugins(IConfigService configService)
        {
            var pluginManager = PluginManager;
            pluginManager.PluginUnloaded += ManagerPluginUnloaded;
            pluginManager.AssemblePlugins();

            var guids = configService.Config.ApplicationPlugins;
            if (guids != null)
            {
                _pluginManager.RestoreApplicationPlugins(guids, this);
            }
        }

        private void ManagerPluginUnloaded(object sender, PluginEventArgs e)
        {
            Toolbars.RemoveItemsForPlugin(e.Identity);
            Menu.RemoveItemsForPlugin(e.Identity);
            DockPanels.RemoveItemsForPlugin(e.Identity);
            Toolbox.RemoveItemsForPlugin(e.Identity);
            StatusBar.RemoveItemsForPlugin(e.Identity);
        }

        public IApplicationContainer Container
        {
            get { return _container; }
        }

        public IProject Project
        {
            get { return _project as IProject; }
        }
        
        public IAppView View
        {
            get { return _view; }
        }

        public IMuteMap Map
        {
            get { return _map; }
        }

        public IMuteLegend Legend
        {
            get { return _legendPresenter.Legend; }
        }

        public IStatusBar StatusBar
        {
            get { return _statusBar; }
        }

        public IMenu Menu
        {
            get { return _menu; }
        }

        public IToolbarCollection Toolbars
        {
            get { return _toolbars; }
        }

        public ILayerCollection<ILayer> Layers
        {
            get { return _map.Layers; }
        }

        public IDockPanelCollection DockPanels
        {
            get { return _dockPanelCollection; }
        }

        public IProjectionDatabase Projections
        {
            get { return _projectionDatabase; }
        }

        public AppConfig Config
        {
            get { return _configService.Config; }
        }

        public ITaskCollection Tasks
        {
            get { return _tasks; }
        }

        public IRepository Repository
        {
            get { return _repository; }
        }

        public SynchronizationContext SynchronizationContext
        {
            get { return _synchronizationContext; }
        }

        public void SetMapProjection(ISpatialReference projection)
        {
            this.SetProjection(projection);
            Map.Redraw();
            View.Update();
        }

        public bool Initialized
        {
            get { return _initialized; }
        }

        public ILocator Locator
        {
            get { return _locator; }
        }

        public IToolbox Toolbox
        {
            get { return _toolboxPresenter.View; }
        }

        public IPluginManager PluginManager
        {
            get { return _pluginManager; }
        }

        public Control GetDockPanelObject(DefaultDockPanel panel)
        {
            switch (panel)
            {
                case DefaultDockPanel.Legend:
                    return _legendPresenter.Legend as Control;
                case DefaultDockPanel.Toolbox:
                    return _toolboxPresenter.View as Control;
                case DefaultDockPanel.Locator:
                    return _locator != null ? _locator.GetInternalObject() : null;
                default:
                    throw new ArgumentOutOfRangeException("panel");
            }
        }

        public IBroadcasterService Broadcaster
        {
            get { return _broadcaster; }
        }

        public void Close()
        {
            _mainView.Close();
        }
    }
}
