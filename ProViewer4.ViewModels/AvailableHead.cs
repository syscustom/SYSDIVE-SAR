
namespace ProViewer4.ViewModels
{
    public class AvailableHead
    {
        public string Name
        {
            get;
            private set;
        }

        public int Index
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return Name;
        }

        public AvailableHead(string name, int index)
        {
            Name = name;
            Index = index;
        }
    }
}
