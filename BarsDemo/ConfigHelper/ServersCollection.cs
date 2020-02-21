using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarsDemo.ConfigHelper
{
    public class ServersCollection : ConfigurationElementCollection
    {
        // Создание нового элемента коллекции серверов
        protected override ConfigurationElement CreateNewElement()
        {
            return new ServerElement();
        }

        // Получение ключа элемента для его идентификации
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ServerElement)element).Name;
        }

        // Получение элемента по его индексу
        public ServerElement this[Int32 index]
        {
            get { return (ServerElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);
                BaseAdd(index, value);
            }
        }

        // Получение элемента по его имени
        public new ServerElement this[String name]
        {
            get { return (ServerElement)BaseGet(name); }
        }
    }
}
