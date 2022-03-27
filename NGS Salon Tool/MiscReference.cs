using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGS_Salon_Tool
{
    public static class MiscReference
    {
        public static List<string> proportionStrings = new List<string>() 
        { 
            "Body",
            "Arm",
            "Leg",
            "Bust",
            "Head",
            "Face Shape",
            "Eye Shape",
            "Nose Height",
            "Nose Shape",
            "Mouth",
            "Ears",
            "Neck",
            "Waist",
            "Hands",
            "Horns",
            "Alt Face Head",
            "Alt Face Face Shape",
            "Alt Face Eye Shape",
            "Alt Face Nose Height",
            "Alt Face Nose Shape",
            "Alt Face Mouth",
            "Alt Face Ears",
            "Alt Face Neck",
            "Alt Face Horns",
            "Alt Face Unknown",
        };

        public static List<string> faceExpressionStrings = new List<string>()
        {
            "Natural",
            "Smile",
            "Angry",
            "Sad",
            "Sus",
            "Eyes Closed",
            "Smile 2",
            "Wink",
            "Unused 1",
            "Unused 2",
        };

        public static List<string> paintPriorityStrings = new List<string>()
        {
            "Innerwear",
            "Body Paint 1",
            "Body Paint 2"
        };

    }
}
