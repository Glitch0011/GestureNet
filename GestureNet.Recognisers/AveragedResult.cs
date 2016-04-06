namespace GestureNet.Recognisers
{
    public class AveragedResult
    {
        public string Name;
        public float Score;

        public override string ToString()
        {
            return Name + ": " + Score;
        }
    }
}