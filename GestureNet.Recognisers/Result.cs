namespace GestureNet.Recognisers
{
    public class Result
    {
        public string Name;
        public float Score;

        public override string ToString()
        {
            return Name + ": " + Score;
        }
    }
}