namespace Tug.TestExt
{
    public interface IThingy : Tug.Ext.IProviderProduct
    {
        void SetThing(string value);

        string GetThing();
    }
}