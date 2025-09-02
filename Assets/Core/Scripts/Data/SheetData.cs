using System.Collections.Generic;

namespace Core.Scripts.Data
{
    public abstract class SheetData
    {
        protected int ID = 0;

        public abstract Dictionary<long, SheetData> LoadData();
    }
}