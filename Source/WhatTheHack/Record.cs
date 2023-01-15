namespace WhatTheHack;

public class Record
{
    public bool isException;
    public bool isSelected;
    public string label = "";

    public Record()
    {
    }

    public Record(bool isSelected, bool isException, string label)
    {
        this.isException = isException;
        this.isSelected = isSelected;
        this.label = label;
    }

    public override string ToString()
    {
        return $"{isSelected},{isException},{label}";
    }
}