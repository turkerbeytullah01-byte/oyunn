// ============================================================================
// Project Aegis: Drone Dominion
// ServiceLocator - Central service provider pattern implementation
// ============================================================================
// Provides a centralized registry for game services, enabling loose coupling
// between components. Services can be registered and retrieved by type.
// Supports both singleton services and transient service creation.
// ============================================================================

using UnityEngine;
using System;
using System.Collections.Generic;

namespace ProjectAegis.Core
{
    /// <summary>
    /// Central service locator for accessing game services.
    /// Provides type-safe service registration and retrieval.
    /// </summary>
    public class ServiceLocator : BaseManager<ServiceLocator>
    {
        #region Configuration
        
        public override int InitializationPriority => 1; // Initialize early
        
        /// <summary>
        /// Whether to log service registration and retrieval.
        /// </summary>
        [SerializeField]
        private bool _logServiceAccess = false;
        
        #endregion
        
        #region Service Registry
        
        /// <summary>
        /// Dictionary storing registered services by type.
        /// </summary>
        private Dictionary<Type, object> _services = new Dictionary<Type, object>();
        
        /// <summary>
        /// Dictionary storing service factories for lazy initialization.
        /// </summary>
        private Dictionary<Type, Func<object>> _serviceFactories = new Dictionary<Type, Func<object>>();
        
        /// <summary>
        /// Dictionary storing service metadata.
        /// </summary>
        private Dictionary<Type, ServiceInfo> _serviceInfo = new Dictionary<Type, ServiceInfo>();
        
        /// <summary>
        /// Information about a registered service.
        /// </summary>
        private class ServiceInfo
        {
            public string Name;
            public DateTime RegistrationTime;
            public bool IsLazy;
            public bool IsSingleton;
        }
        
        #endregion
        
        #region Registration
        
        /// <summary>
        /// Registers a service instance.
        /// </summary>
        /// <typeparam name="T">The service interface type</typeparam>
        /// <param name="service">The service instance</param>
        /// <param name="asSingleton">If true, the service is treated as a singleton</param>
        public void Register<T>(T service, bool asSingleton = true) where T : class
        {
            var type = typeof(T);
            
            if (service == null)
            {
                LogError($"Cannot register null service of type {type.Name}");
                return;
            }
            
            if (_services.ContainsKey(type))
            {
                LogWarning($"Service of type {type.Name} is already registered. Overwriting.");
            }
            
            _services[type] = service;
            _serviceInfo[type] = new ServiceInfo
            {
                Name = type.Name,
                RegistrationTime = DateTime.UtcNow,
                IsLazy = false,
                IsSingleton = asSingleton
            };
            
            if (_logServiceAccess)
            {
                Log($"Registered service: {type.Name}");
            }
        }
        
        /// <summary>
        /// Registers a service factory for lazy initialization.
        /// The factory is called when the service is first requested.
        /// </summary>
        /// <typeparam name="T">The service interface type</typeparam>
        /// <param name="factory">Factory function that creates the service</param>
        /// <param name="asSingleton">If true, the factory is only called once</param>
        public void RegisterLazy<T>(Func<T> factory, bool asSingleton = true) where T : class
        {
            var type = typeof(T);
            
            if (factory == null)
            {
                LogError($"Cannot register null factory for type {type.Name}");
                return;
            }
            
            if (_serviceFactories.ContainsKey(type))
            {
                LogWarning($"Factory for type {type.Name} is already registered. Overwriting.");
            }
            
            _serviceFactories[type] = () => factory();
            _serviceInfo[type] = new ServiceInfo
            {
                Name = type.Name,
                RegistrationTime = DateTime.UtcNow,
                IsLazy = true,
                IsSingleton = asSingleton
            };
            
            if (_logServiceAccess)
            {
                Log($"Registered lazy service: {type.Name}");
            }
        }
        
        /// <summary>
        /// Unregisters a service.
        /// </summary>
        /// <typeparam name="T">The service interface type</typeparam>
        public void Unregister<T>() where T : class
        {
            var type = typeof(T);
            
            if (_services.Remove(type) || _serviceFactories.Remove(type))
            {
                _serviceInfo.Remove(type);
                
                if (_logServiceAccess)
                {
                    Log($"Unregistered service: {type.Name}");
                }
            }
        }
        
        /// <summary>
        /// Checks if a service is registered.
        /// </summary>
        /// <typeparam name="T">The service interface type</typeparam>
        public bool IsRegistered<T>() where T : class
        {
            var type = typeof(T);
            return _services.ContainsKey(type) || _serviceFactories.ContainsKey(type);
        }
        
        #endregion
        
        #region Retrieval
        
        /// <summary>
        /// Gets a registered service.
        /// </summary>
        /// <typeparam name="T">The service interface type</typeparam>
        /// <returns>The service instance</returns>
        /// <exception cref="InvalidOperationException">If service is not registered</exception>
        public T Get<T>() where T : class
        {
            var type = typeof(T);
            
            // Check for existing instance
            if (_services.TryGetValue(type, out var service))
            {
                if (_logServiceAccess)
                {
                    Log($"Retrieved service: {type.Name}");
                }
                return (T)service;
            }
            
            // Check for lazy factory
            if (_serviceFactories.TryGetValue(type, out var factory))
            {
                service = factory();
                
                if (service == null)
                {
                    throw new InvalidOperationException($"Factory for service {type.Name} returned null");
                }
                
                // Cache if singleton
                var info = _serviceInfo[type];
                if (info.IsSingleton)
                {
                    _services[type] = service;
                    _serviceFactories.Remove(type);
                }
                
                if (_logServiceAccess)
                {
                    Log($"Created lazy service: {type.Name}");
                }
                
                return (T)service;
            }
            
            throw new InvalidOperationException($"Service of type {type.Name} is not registered");
        }
        
        /// <summary>
        /// Tries to get a registered service.
        /// </summary>
        /// <typeparam name="T">The service interface type</typeparam>
        /// <param name="service">Output parameter for the service</param>
        /// <returns>True if the service was found</returns>
        public bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            
            // Check for existing instance
            if (_services.TryGetValue(type, out var existing))
            {
                service = (T)existing;
                return true;
            }
            
            // Check for lazy factory
            if (_serviceFactories.TryGetValue(type, out var factory))
            {
                var instance = factory();
                
                if (instance == null)
                {
                    service = null;
                    return false;
                }
                
                // Cache if singleton
                var info = _serviceInfo[type];
                if (info.IsSingleton)
                {
                    _services[type] = instance;
                    _serviceFactories.Remove(type);
                }
                
                service = (T)instance;
                return true;
            }
            
            service = null;
            return false;
        }
        
        /// <summary>
        /// Gets a service or returns a default value if not registered.
        /// </summary>
        /// <typeparam name="T">The service interface type</typeparam>
        /// <param name="defaultValue">Default value if service not found</param>
        /// <returns>The service or default value</returns>
        public T GetOrDefault<T>(T defaultValue = null) where T : class
        {
            return TryGet(out T service) ? service : defaultValue;
        }
        
        /// <summary>
        /// Gets a service or creates it using the provided factory if not registered.
        /// </summary>
        /// <typeparam name="T">The service interface type</typeparam>
        /// <param name="factory">Factory to create the service if not found</param>
        /// <returns>The service instance</returns>
        public T GetOrCreate<T>(Func<T> factory) where T : class
        {
            if (TryGet(out T service))
            {
                return service;
            }
            
            service = factory();
            if (service != null)
            {
                Register(service);
            }
            
            return service;
        }
        
        #endregion
        
        #region Service Management
        
        /// <summary>
        /// Gets all registered service types.
        /// </summary>
        public Type[] GetAllServiceTypes()
        {
            var types = new List<Type>();
            types.AddRange(_services.Keys);
            types.AddRange(_serviceFactories.Keys);
            return types.ToArray();
        }
        
        /// <summary>
        /// Gets information about all registered services.
        /// </summary>
        public string GetServiceReport()
        {
            var report = "=== Service Locator Report ===\n";
            
            foreach (var kvp in _serviceInfo)
            {
                var info = kvp.Value;
                report += $"[{kvp.Key.Name}] ";
                report += $"Lazy:{info.IsLazy} ";
                report += $"Singleton:{info.IsSingleton} ";
                report += $"Registered:{info.RegistrationTime:HH:mm:ss}\n";
            }
            
            return report;
        }
        
        /// <summary>
        /// Clears all registered services.
        /// </summary>
        public void ClearAllServices()
        {
            _services.Clear();
            _serviceFactories.Clear();
            _serviceInfo.Clear();
            
            Log("All services cleared");
        }
        
        #endregion
        
        #region Initialization
        
        protected override void OnInitialize()
        {
            // ServiceLocator is ready to accept registrations
            Log("ServiceLocator initialized");
        }
        
        #endregion
    }
    
    #region Extension Methods
    
    /// <summary>
    /// Extension methods for easier service access.
    /// </summary>
    public static class ServiceLocatorExtensions
    {
        /// <summary>
        /// Gets a service from the ServiceLocator.
        /// </summary>
        public static T GetService<T>(this MonoBehaviour behaviour) where T : class
        {
            return ServiceLocator.Instance.Get<T>();
        }
        
        /// <summary>
        /// Tries to get a service from the ServiceLocator.
        /// </summary>
        public static bool TryGetService<T>(this MonoBehaviour behaviour, out T service) where T : class
        {
            return ServiceLocator.Instance.TryGet(out service);
        }
    }
    
    #endregion
}
