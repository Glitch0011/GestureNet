using System.Collections.Generic;

namespace GestureNet.Recognisers
{
    public class Result
    {
        public string Name;
        public float Score;
        public List<float> SubScores;

        public override string ToString()
        {
            return Name + ": " + Score;
        }
    }
}