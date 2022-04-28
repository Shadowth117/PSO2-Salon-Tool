using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharFileLibrary.Character_Making_File_Tool.Constants
{
    public static class CharacterProportionConstants
    {
        //Below are the keyframes at which values for specific sliders are stored

        //Body
        public static int PhysiqueCenterYMin = 10;
        public static int PhysiqueCenterYMax = 12;
        public static int PhysiqueSideXMin = 14;
        public static int PhysiqueSideXMax = 16;
        public static int PhysiqueSideYMin = 18;
        public static int PhysiqueSideYMax = 20;
        public static int ArmsCenterYMin = 22;
        public static int ArmsCenterYMax = 24;
        public static int ArmsSideXMin = 26;
        public static int ArmsSideXMax = 28;
        public static int ArmsSideYMin = 30;
        public static int ArmsSideYMax = 32;
        public static int LegsCenterYMin = 34;
        public static int LegsCenterYMax = 36;
        public static int LegsSideXMin = 38;
        public static int LegsSideXMax = 40;
        public static int LegsSideYMin = 42;
        public static int LegsSideYMax = 44;
        public static int ChestCenterYMin = 46;
        public static int ChestCenterYMax = 48;
        public static int ChestSideXMin = 50;
        public static int ChestSideXMax = 52;
        public static int ChestSideYMin = 54;
        public static int ChestSideYMax = 56;
        public static int NeckCenterYMin = 58;
        public static int NeckCenterYMax = 60;
        public static int NeckSideXMin = 62;
        public static int NeckSideXMax = 64;
        public static int NeckSideYMin = 66;
        public static int NeckSideYMax = 68;
        public static int WaistCenterYMin = 70;
        public static int WaistCenterYMax = 72;
        public static int WaistSideXMin = 74;
        public static int WaistSideXMax = 76;
        public static int WaistSideYMin = 78;
        public static int WaistSideYMax = 80;

        //NGS Body
        public static int ShoulderSizeMin = 6;
        public static int ShoulderSizeMax = 8;
        public static int HandsCenterYMin = 82;
        public static int HandsCenterYMax = 84;
        public static int HandsSideXMin = 86;
        public static int HandsSideXMax = 88;
        public static int HandsSideYMin = 90;
        public static int HandsSideYMax = 92;
        public static int NeckAngleMin = 94;
        public static int NeckAngleMax = 96;
        public static int VerticalPositionArmsMin = 98;
        public static int VerticalPositionArmsMax = 100;
        public static int ThighsMin = 102;
        public static int ThighsMax = 104;
        public static int CalvesMin = 106;
        public static int CalvesMax = 108;
        public static int ForearmsMin = 110;
        public static int ForearmsMax = 112;
        public static int HandThicknessMin = 114;
        public static int HandThicknessMax = 116;
        public static int FootSizeMin = 118;
        public static int FootSizeMax = 120;

        //Face
        public static int NoseSizeSideYMin = 6;
        public static int NoseSizeSideYMax = 8;
        public static int FaceShape1CenterYMin = 10;
        public static int FaceShape1CenterYMax = 12;
        public static int FaceShape1SideXMin = 14;
        public static int FaceShape1SideXMax = 16;
        public static int FaceShape1SideYMin = 18;
        public static int FaceShape1SideYMax = 20;
        public static int FaceShape2CenterYMin = 22;
        public static int FaceShape2CenterYMax = 24;
        public static int FaceShape2SideXMin = 26;
        public static int FaceShape2SideXMax = 28;
        public static int FaceShape2SideYMin = 30;
        public static int FaceShape2SideYMax = 32;
        public static int EyesCenterYMin = 34;
        public static int EyesCenterYMax = 36;
        public static int EyesSideXMin = 38;
        public static int EyesSideXMax = 40;
        public static int EyesSideYMin = 42;
        public static int EyesSideYMax = 44;
        public static int NoseSizeCenterYMin = 46;
        public static int NoseSizeCenterYMax = 48;
        public static int NoseSizeSideXMin = 50;
        public static int NoseSizeSideXMax = 52;
        public static int NoseLengthCenterYMin = 54;
        public static int NoseLengthCenterYMax = 56;
        public static int NoseLengthSideXMin = 58;
        public static int NoseLengthSideXMax = 60;
        public static int NoseLengthSideYMin = 62;
        public static int NoseLengthSideYMax = 64;
        public static int MouthShapeCenterYMin = 66;
        public static int MouthShapeCenterYMax = 68;
        public static int MouthShapeSideXMin = 70;
        public static int MouthShapeSideXMax = 72;
        public static int MouthShapeSideYMin = 74;
        public static int MouthShapeSideYMax = 76;

        //Old Face (Horns/Ears were attached to the face in old pso2)

        //NGS Face
        public static int EyeHorizontalPositionMin = 82;
        public static int EyeHorizontalPositionMax = 84;
        public static int UnusedMin = 86; //Does nothing
        public static int UnusedMax = 88; //Does nothing
        public static int EyeSizeMin = 90; //CorrelatestoIrisbone,butdoesnothing?
        public static int EyeSizeMax = 92; //CorrelatestoIrisbone,butdoesnothing?
        public static int LeftEyebrowVerticalPositionMin = 94;
        public static int LeftEyebrowVerticalPositionMax = 96;
        public static int LeftMouthVerticalPositionMin = 98;
        public static int LeftMouthVerticalPositionMax = 100;
        public static int RightEyebrowVerticalPositionMin = 102;
        public static int RightEyebrowVerticalPositionMax = 104;
        public static int RightMouthVerticalPositionMin = 106;
        public static int RightMouthVerticalPositionMax = 108;
        public static int EyeCornerMin = 110;
        public static int EyeCornerMax = 112;
        public static int LeftEyelidVerticalPositionMin = 114;
        public static int LeftEyelidVerticalPositionMax = 116;
        public static int LeftEyebrowExpressionMin = 118;
        public static int LeftEyebrowExpressionMax = 120;
        public static int RightEyelidVerticalPositionMin = 122;
        public static int RightEyelidVerticalPositionMax = 124;
        public static int RightEyebrowExpressionMin = 126;
        public static int RightEyebrowExpressionMax = 128;
        public static int MouthTypeA = 130;
        public static int MouthTypeI = 132;
        public static int MouthTypeU = 134;
        public static int MouthTypeE = 136;
        public static int MouthTypeO = 138;
        public static int UnusedLeftEyebrowVertical = 140; //Unused, but works ingame
        public static int UnusedRightEyebrowVertical = 142; //Unused, but works ingame
        public static int Tongue = 144;
        public static int MouthVerticalPositionMin = 146;
        public static int MouthVerticalPositionMax = 148;
        public static int EyebrowHorizontalPositionMin = 150;
        public static int EyebrowHorizontalPositionMax = 152;
        public static int IrisVerticalPositionMin = 154;
        public static int IrisVerticalPositionMax = 156;

        //V11 char file+
        public static int TongueVerticalMin = 158;
        public static int TongueVerticalMax = 160;
        public static int TongueHorizontalMin = 162;
        public static int TongueHorizontalMax = 164;
    }
}
