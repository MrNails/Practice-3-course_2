using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Практика2Курс
{
    class PhoneCollection<T> : List<T>
        where T: Phone 
    {
        public PhoneCollection() : base()
        {}

        public PhoneCollection(int capacity) : base(capacity)
        {}

        public PhoneCollection(IEnumerable<T> collection) : base(collection)
        {}

        public new void Add(T item)
        {
            if (item.IsSelected) {
                foreach (var elem in this)
                {
                    if (elem.IsSelected)
                    {
                        elem.IsSelected = false;
                    }
                }
            }
            base.Add(item);
        }

        public T FindSelectedItem()
        {
            foreach (var elem in this)
            {
                if (elem.IsSelected)
                {
                    return elem;
                }
            }
            
            return null;
        }
    }
}
