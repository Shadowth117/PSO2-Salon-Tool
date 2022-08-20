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
            "NGS Face Head",
            "NGS Face Face Shape",
            "NGS Face Eye Shape",
            "NGS Face Nose Height",
            "NGS Face Nose Shape",
            "NGS Face Mouth",
            "NGS Face Ears",
            "NGS Face Neck",
            "Waist",
            "Hands",
            "NGS Face Horns",
            "Classic Face Head",
            "Classic Face Face Shape",
            "Classic Face Eye Shape",
            "Classic Face Nose Height",
            "Classic Face Nose Shape",
            "Classic Face Mouth",
            "Classic Face Ears",
            "Classic Face Neck",
            "Classic Face Horns",
            "Classic Face Unknown",
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
