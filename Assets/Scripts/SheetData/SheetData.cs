using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class SheetData
{
    protected int ID = 0;

    public abstract Dictionary<long, SheetData> LoadData();
}