﻿using System;
using ResourceManagement.Resources;

namespace ResourceManagement.Loaders
{
    public abstract class ABasicLoader : IResourceLoader
    {
        ResourceHandle defaultResource;

        abstract public string Type { get; }

        public ResourceHandle Default
        {
            get
            {
                if (defaultResource == null) {
                    loadDefault();
                }
                return defaultResource;
            }
        }

        abstract protected AResource doLoad(string name);
        abstract protected void doUnload(AResource resource);

        public void Load(ResourceHandle handle)
        {
            try {
                handle.inactive.resource = doLoad(handle.Name);
                handle.inactive.state = ResourceState.Ready;
                handle.Swap();

            }
            finally {
                handle.Finished();
            }
        }
        
        public void Load(ResourceHandle handle, IEvent evt)
        {
            try
            {
                Load(handle);
                evt.Finish();
            }
            catch(Exception)
            {
                evt.Abort();
            }
        }

        public void Unload(ResourceHandle handle)
        {
            try {
                doUnload(handle.inactive.resource);
                handle.inactive.resource = null;
                handle.inactive.state = ResourceState.Empty;
            }
            finally {
                handle.Finished();
            }
        }

        public void Unload(ResourceHandle handle, IEvent evt)
        {
            try
            {
                Unload(handle);
                evt.Finish();
            }
            catch (Exception)
            {
                evt.Abort();
            }
        }

        public void Reload(ResourceHandle handle)
        {
            try {
                doUnload(handle.inactive.resource);
                handle.inactive.state = ResourceState.Loading;
                handle.inactive.resource = doLoad(handle.Name);

                handle.Swap();
                handle.active.state = ResourceState.Ready;
            }
            finally {
                handle.Finished();
            }
        }

        public void Reload(ResourceHandle handle, IEvent evt)
        {
            try
            {
                Reload(handle);
                evt.Finish();
            }
            catch (Exception)
            {
                evt.Abort();
            }
        }

        private void loadDefault()
        {
            defaultResource = new ResourceHandle("default", this);
            IEvent evt = new BasicEvent();
            defaultResource.Load(evt);
            evt.Wait();
        }

        ~ABasicLoader()
        {
            if (defaultResource != null) {
                IEvent evt = new BasicEvent();
                defaultResource.Unload(evt);
                evt.Wait();
            }
        }
    }
}
