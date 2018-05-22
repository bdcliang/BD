using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;

namespace BD.Services
{
    public class ServiceInstaller
    {
        /// <summary>  
        /// check the the given service if exisit 
        /// </summary>  
        /// <param name=" NameService ">服务名</param>  
        /// <returns>存在返回 true,否则返回 false;</returns>  
        public static bool ServiceIsExisted(string NameService)
        {
            ServiceController[] services = ServiceController.GetServices();
            foreach (ServiceController s in services)
            {
                if (s.ServiceName.ToLower() == NameService.ToLower())
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>  
        /// install the given services 
        /// </summary>  
        /// <param name="stateSaver">集合</param>  
        /// <param name="filepath">程序文件路径</param>  
        public static void InstallService(string filepath)
        {
            AssemblyInstaller AssemblyInstaller1 = new AssemblyInstaller();
            AssemblyInstaller1.UseNewContext = true;
            AssemblyInstaller1.Path = filepath;
            AssemblyInstaller1.Install(null);
            AssemblyInstaller1.Commit(null);
            AssemblyInstaller1.Dispose();
        }
        /// <summary>  
        /// uninstall services  
        /// </summary>  
        /// <param name="filepath">程序文件路径</param>  
        public static void UnInstallService(string filepath)
        {
            AssemblyInstaller AssemblyInstaller1 = new AssemblyInstaller();
            AssemblyInstaller1.UseNewContext = true;
            AssemblyInstaller1.Path = filepath;
            AssemblyInstaller1.Uninstall(null);
            AssemblyInstaller1.Dispose();
        }

        /// <summary>  
        /// start the service  
        /// </summary>  
        /// <param name=" NameService ">服务名</param>  
        /// <returns>存在返回 true,否则返回 false;</returns>  
        public static bool RunService(string NameService)
        {
            bool bo = true;
            try
            {
                ServiceController sc = new ServiceController(NameService);
                if (sc.Status.Equals(ServiceControllerStatus.Stopped) || sc.Status.Equals(ServiceControllerStatus.StopPending))
                {
                    sc.Start();
                }
            }
            catch (Exception ex)
            {
                bo = false;
            }

            return bo;
        }

        /// <summary>  
        /// stop services 
        /// </summary>  
        /// <param name=" NameService ">服务名</param>  
        /// <returns>存在返回 true,否则返回 false;</returns>  
        public static bool StopService(string NameService)
        {
            bool bo = true;
            try
            {
                ServiceController sc = new ServiceController(NameService);
                if (!sc.Status.Equals(ServiceControllerStatus.Stopped))
                {
                    sc.Stop();
                }
            }
            catch (Exception ex)
            {
                bo = false;
                //LogAPI.WriteLog(ex.Message);
            }

            return bo;
        }

        /// <summary>  
        /// get the status of the service  
        /// </summary>  
        /// <param name=" NameService ">服务名</param>  
        /// <returns>返回服务状态</returns>  
        public static int GetServiceStatus(string NameService)
        {
            int ret = 0;
            try
            {
                ServiceController sc = new ServiceController(NameService);
                ret = Convert.ToInt16(sc.Status);
            }
            catch (Exception ex)
            {
                ret = 0;
                //LogAPI.WriteLog(ex.Message);
            }

            return ret;
        }

        /// <summary>  
        /// get the installed service path  
        /// </summary>  
        /// <param name="ServiceName"></param>  
        /// <returns></returns>  
        public static string GetWindowsServiceInstallPath(string ServiceName)
        {
            string path = "";
            try
            {
                string key = @"SYSTEM\CurrentControlSet\Services\" + ServiceName;
                path = Registry.LocalMachine.OpenSubKey(key).GetValue("ImagePath").ToString();

                path = path.Replace("\"", string.Empty);//替换掉双引号    

                FileInfo fi = new FileInfo(path);
                path = fi.Directory.ToString();
            }
            catch (Exception ex)
            {
                path = "";
                //LogAPI.WriteLog(ex.Message);
            }
            return path;
        }

        /// <summary>  
        /// get the version 
        /// </summary>  
        /// <param name="serviceName">服务名称</param>  
        /// <returns></returns>  
        public static string GetServiceVersion(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                return string.Empty;
            }
            try
            {
                string path = GetWindowsServiceInstallPath(serviceName) + "\\" + serviceName + ".exe";
                Assembly assembly = Assembly.LoadFile(path);
                AssemblyName assemblyName = assembly.GetName();
                Version version = assemblyName.Version;
                return version.ToString();
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
 }
