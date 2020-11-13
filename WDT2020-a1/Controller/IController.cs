using System;
using System.Collections.Generic;

namespace WDT2020_a1.Controller
{
    public interface IController <T>
    {
        public void Insert(T item);

        public List<T> GetAll(int id);

        public T Get(int id);

        public void Update(T item);

        public bool DoesExist(int id);
    }
}
