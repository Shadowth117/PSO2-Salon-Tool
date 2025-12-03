using static Character_Making_File_Tool.CharacterDataStructs;

namespace Character_Making_File_Tool
{
    public static class CharacterStructConstants
    {
        public static FaceExpressionV12[] defaultMaleExpressions = new FaceExpressionV12[]
        {
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 4, -109, -77, -110, -77, 127, -36, -24, -35, -24, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Natural
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 0, -94, 11, -94, 11, 44, -34, -18, -34, -18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Smile
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { -22, -127, -105, -127, -105, 127, -39, -78, -39, -78, 0, 0, 0, 0, 14, 0, 0, 0, 0, 0 } ), //Angry
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 0, 47, -127, 47, -127, 33, -43, -109, -43, -109, 23, 0, 0, 9, 0, 0, 0, 0, 0, 0 } ), //Sad
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 0, 127, -87, -109, -87, 127, -28, -56, -41, -56, 66, 0, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Sus
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 0, -100, -81, -100, -81, 127, -127, -78, -127, -78, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Eyes Closed
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 0, -104, 61, -104, 61, 127, -21, 46, -21, 46, 0, 89, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Smile 2
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 0, -103, 9, -127, 34, 101, -31, 22, -127, -63, 0, 45, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Wink
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Custom 1
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Custom 2
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Custom 3
        };

        public static FaceExpressionV12[] defaultFemaleExpressions = new FaceExpressionV12[]
        {
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 0, 0, -27, 0, -27, -75, -7, 0, -6, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Natural
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 0, -81, 29, -81, 29, -70, -11, 57, -11, 57, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Smile
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 0, -53, -107, -53, -107, 3, -9, -68, -9, -68, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Angry
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 3, 127, -84, 127, -84, 5, -22, -73, -22, -73, 30, 0, 0, 9, 0, 0, 0, 0, 0, 0 } ), //Sad
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 3, -1, -65, -1, -65, 5, -24, 30, -24, -114, 0, 0, 0, 43, 0, 0, 0, 0, 0, 0 } ), //Sus
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 0, 0, 0, 0, 0, 0, -127, 0, -127, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Eyes Closed
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 2, -42, 39, -42, 39, -74, 9, 19, 9, 19, 0, 56, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Smile 2
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 2, -35, 93, 29, 37, 0, -127, 44, 0, 110, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Wink
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Custom 1
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Custom 2
            FaceExpressionV12.CreateExpression(new sbyte[0x14] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } ), //Custom 3
        };
    }
}
