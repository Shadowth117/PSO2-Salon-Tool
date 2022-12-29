using AquaModelLibrary;
using AquaModelLibrary.Native.Fbx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using Zamboni;
using static AquaExtras.FilenameConstants;
using static AquaModelLibrary.AquaMethods.AquaGeneralMethods;
using static AquaModelLibrary.CharacterMakingIndex;
using static AquaModelLibrary.Extra.ReferenceGenerator;
using static AquaModelLibrary.Utility.AquaUtilData;
using static Character_Making_File_Tool.CharacterConstants;
using static Character_Making_File_Tool.CharacterHandlerReboot;
using static CharFileLibrary.Character_Making_File_Tool.Constants.CharacterProportionConstants;

namespace CharFileLibrary.Character_Making_File_Tool.Utilities
{
    public class ModelUtility
    {
        private xxpGeneralReboot xxp = null;
        private CharacterMakingIndex cmx = null;
        private string pso2_binDir = null;
        public const string basewearStr = "basewear";
        public const string costumeStr = "costume";
        public const string castStr = "cast";
        public string basewearFilename = null;
        public string basewearRPFilename = null;
        public string basewearHNFilename = null;
        public string basewearLinkedInnerFilename = null;

        public AquaObject compositeModel = null;
        public AquaObject basewearModel = null;
        public AquaNode compositeBones = null;
        public AquaNode basewearNode = null;

        public class TransformSet
        {
            public ushort boneShort1;
            public ushort boneShort2;
            public Vector3 basePos = new Vector3();
            public Vector3 pos = new Vector3();
            public Quaternion baseRot = Quaternion.Identity;
            public Quaternion rot = Quaternion.Identity;
            public Vector3 scale = new Vector3(1, 1, 1);
            public Vector3 postScale = new Vector3(1, 1, 1);

            public TransformSet()
            {

            }

            public TransformSet(Matrix4x4 mat)
            {
                Matrix4x4.Decompose(mat, out var matScale, out var matRot, out var matPos);
                pos = matPos;
                rot = matRot;
                scale = matScale;
            }

            public TransformSet Clone()
            {
                var tfm = new TransformSet();
                tfm.pos = pos;
                tfm.rot = rot;
                tfm.scale = scale;

                return tfm;
            }

            public void PSO2Transform(TransformSet oldParSet, TransformSet newParSet, TransformSet relativeTfm, TransformSet relativeParTfm)
            {
                //Rotation transformation
                //Remove old parent transform's influence, apply relative rotation, apply new parent transform's influence
                rot = rot * Quaternion.Inverse(oldParSet.rot);
                rot = rot * relativeTfm.rot;
                rot = rot * newParSet.rot;

                //Translation vector transformation
                //We remove the old parent transform's influence from it, add the relative position, and multiply by the parent's scale before adding back the new parent transform influence
                pos -= oldParSet.pos;

                pos = Vector3.Transform(pos, Quaternion.Inverse(oldParSet.rot));
                pos += relativeTfm.pos;
                pos *= relativeParTfm.scale; 

                var bs1 = boneShort1;
                if ((bs1 & 0x40) < 1 && (bs1 & 0x80) < 1 && (bs1 & 0x100) < 1 && (boneShort2 & 0x400) > 0)
                {
                    pos *= relativeTfm.postScale;
                }

                pos = Vector3.Transform(pos, newParSet.rot);

                pos += newParSet.pos;

                //Scale influence isn't done the usual way in pso2 so we leave this out.
            }

            public Matrix4x4 GetMatrix4x4(bool nullPos = false)
            {
                Matrix4x4 mat = Matrix4x4.Identity;

                mat *= Matrix4x4.CreateScale(scale);
                mat *= Matrix4x4.CreateFromQuaternion(rot);
                if (nullPos == false)
                {
                    mat *= Matrix4x4.CreateTranslation(pos);
                }

                return mat;
            }

            public Matrix4x4 GetInverseMatrix4x4(bool nullPos = false)
            {
                Matrix4x4.Invert(GetMatrix4x4(nullPos), out Matrix4x4 result);
                return result;
            }
        }

        public ModelUtility(string pso2_binDirectory)
        {
            pso2_binDir = pso2_binDirectory;
        }

        public void GetCharacterModel(xxpGeneralReboot xxpIn, CharacterMakingIndex cmxIn)
        {
            xxp = xxpIn;
            cmx = cmxIn;
            //Load Proportions ICE
            string proportionsPath = Path.Combine(pso2_binDir, dataDir, GetFileHash(classicSystemData));
            string bodyTypeString = GetBodyTypeString(xxp, out string bodyCategory);
            AquaMotion bodyProps = GetMotion(xxp, proportionsPath, bodyTypeString);

            //Get filenames
            GetCharacterFilenames();

            //Load body models
            switch (bodyCategory)
            {
                case basewearStr: //Basewear is base, Outer wear is attached
                    basewearModel = GetModel(xxp, Path.Combine(pso2_binDir, dataDir, basewearFilename), new List<string>() { ".aqp", ".aqn" }, out basewearNode);
                    break;
                case costumeStr: //On its own
                    break;
                case castStr: //Legs are base, Body and Arms are to be attached
                    break;
            }

            compositeModel = basewearModel;
            compositeBones = basewearNode;

            //Calc final transformation matrices per bone
            var finalBodyTransforms = CalcBodyTransforms(bodyProps, compositeBones);
            /*
            //Calc final face transformations per bone
            var finalFaceTransforms = CalcFaceTransforms(faceProps, bodyProps);

            //Calc final mouth transformations per bone
            var finalMouthTransforms = CalcMouthTransforms(mouthProps);

            //Calc final ear transformations per bone
            var finalEarTransforms = CalcEarTransforms(earProps);

            //Calc final face transformations per bone
            var finalHornTransforms = CalcHornTransforms(hornProps);

            //Calc final ngs hair transformations per bone (Seems to use X scale of drs_line_front_1_1 as a vertical scale for bangs on frames 1 and 3)
            var finalHairTransforms = CalcHairTransforms(hornProps);
            */

            var propMotion = ApplyProportionsToAnimation(finalBodyTransforms, compositeBones, bodyProps);
            //ApplyProportions(finalBodyTransforms, compositeModel, compositeBones);

            //Load head models 

            //Load NGS mouth/teeth

            //Load NGS ears

            //Load NGS horns

            //Load accessories - Accessories should scale with their associated parts

            //Assemble model data and bones

            //Blend body textures
            switch (bodyCategory)
            {
                case basewearStr: //Basewear is base, Outer wear is attached. Body paint and inner should be applied 
                    break;
                case costumeStr: //On its own. Inner should not be applied
                    break;
                case castStr: //Legs are base, Body and Arms are to be attached. Body paints and inner should NOT be applied outside of NGS linked inner
                    break;
            }

            //Blend face textures - cast and special head/full coverage hairs should NOT have blending

            //Color accessory textures

            AquaUtil aqu = new AquaUtil();
            ModelSet ms = new ModelSet();
            ms.models.Add(compositeModel);
            aqu.aquaModels.Add(ms);
            if (compositeModel.objc.type >= 0xC32)
            {
                aqu.WriteNGSNIFLModel("C:/Test.aqp", "C:/Test.aqp");
            }
            else
            {
                aqu.WriteClassicNIFLModel("C:/Test.aqp", "C:/Test.aqp");
            }
            AquaUtil.WriteBones("C:/Test.aqn", compositeBones);
            if (aqu.aquaModels[0].models[0].objc.type > 0xC32)
            {
                aqu.aquaModels[0].models[0].splitVSETPerMesh();
            }
            FbxExporter.ExportToFile(aqu.aquaModels[0].models[0], compositeBones, new List<AquaMotion>() { bodyProps }, "C:/TestBaseAnim.fbx", new List<string>() { "CharProportions.aqm" }, true);
            FbxExporter.ExportToFile(aqu.aquaModels[0].models[0], compositeBones, new List<AquaMotion>() { propMotion }, "C:/Test.fbx", new List<string>() { "CharProportions.aqm" }, true);
        }

        public AquaMotion ApplyProportionsToAnimation(List<TransformSet> props, AquaNode aquaNode, AquaMotion proportionAqm)
        {
            AquaMotion motion = new AquaMotion();
            motion.moHeader.endFrame = 1;
            motion.moHeader.frameSpeed = 30;
            motion.moHeader.nodeCount = aquaNode.nodeList.Count;
            for(int i = 0; i < aquaNode.nodeList.Count; i++)
            {
                if(i >= 172)
                {
                    continue;
                }
                if(props.Count <= i)
                {
                    break;
                }
                var prop = props[i];
                var node = aquaNode.nodeList[i];
                AquaMotion.KeyData kd = new AquaMotion.KeyData();
                kd.mseg.nodeName = node.boneName;
                kd.mseg.nodeDataCount = 3;
                kd.mseg.nodeId = i;

                AquaMotion.MKEY posKey = new AquaMotion.MKEY();
                posKey.keyType = 1;
                posKey.dataType = 1;
                posKey.keyCount = 1;
                var finalPos = prop.basePos + prop.pos;
                //if(proportionAqm.motionKeys.Count >= aquaNode.nodeList.Count)
                //{
                    //posKey.vector4Keys.Add(proportionAqm.motionKeys[i].keyData[0].vector4Keys[0]);
                //} else
                //{
                    posKey.vector4Keys.Add(new Vector4((finalPos), 0));
                //}
                
                AquaMotion.MKEY rotKey = new AquaMotion.MKEY();
                rotKey.keyType = 2;
                rotKey.dataType = 3;
                rotKey.keyCount = 1;
                var finalRot = prop.baseRot * prop.rot;
                /*if (proportionAqm.motionKeys.Count >= aquaNode.nodeList.Count)
                {*/
                    rotKey.vector4Keys.Add(proportionAqm.motionKeys[i].keyData[1].vector4Keys[0]);
                /*} else
                {
                    rotKey.vector4Keys.Add(new Vector4(finalRot.X, finalRot.Y, finalRot.Z, finalRot.W));
                }*/

                AquaMotion.MKEY sclKey = new AquaMotion.MKEY();
                sclKey.keyType = 3;
                sclKey.dataType = 1;
                sclKey.keyCount = 1;
                sclKey.vector4Keys.Add(new Vector4(1, 1, 1, 0));
                //sclKey.vector4Keys.Add(new Vector4(prop.scale, 0));

                kd.keyData.Add(posKey);
                kd.keyData.Add(rotKey);
                kd.keyData.Add(sclKey);
                motion.motionKeys.Add(kd);
            }

            return motion;
        }

        public void ApplyProportions(List<TransformSet> props, AquaObject bodyModel, AquaNode aquaNode)
        {
            List<TransformSet> oldBoneList = new List<TransformSet>() { new TransformSet() };
            List<TransformSet> newBoneList = new List<TransformSet>() { new TransformSet() };
            List<Vector3> localPoss = new List<Vector3>() { new Vector3() };
            List<Quaternion> localRots = new List<Quaternion>() { Quaternion.Identity };

            //Gather node transforms, apply node. Start at 1 to skip root.
            for (int i = 1; i < aquaNode.nodeList.Count; i++)
            {
                var node = aquaNode.nodeList[i];
                Matrix4x4.Invert(aquaNode.nodeList[i].GetInverseBindPoseMatrix(), out Matrix4x4 mat);
                var tfmSet = new TransformSet(mat);
                Matrix4x4.Decompose(mat, out var scale, out var rot, out var pos);
                oldBoneList.Add(tfmSet);

                var newTfmSet = tfmSet.Clone();
                var oldParMat = oldBoneList[aquaNode.nodeList[i].parentId];
                var newParMat = newBoneList[aquaNode.nodeList[i].parentId];
                var parProps = new TransformSet();

                if (aquaNode.nodeList[i].parentId < props.Count)
                {
                    parProps = props[aquaNode.nodeList[i].parentId];
                }

                //DEBUG REMOVE LATER ********************************
                /*
                if (props.Count > i)
                {
                    props[i].pos = new Vector3();
                    props[i].rot = Quaternion.Identity;
                    //props[i].scale = scale;
                }*/
                localPoss.Add(tfmSet.pos - oldParMat.pos);
                localRots.Add(tfmSet.rot * Quaternion.Inverse(oldParMat.rot));
                //DEBUG REMOVE LATER ********************************

                if (props.Count > i)
                {
                    newTfmSet.PSO2Transform(oldParMat, newParMat, props[i], parProps);
                }
                else
                {
                    newTfmSet.PSO2Transform(oldParMat, newParMat, new TransformSet(), parProps);
                }
                Debug.WriteLine($"{node.boneName.GetString()} {props[i].postScale.X} {props[i].postScale.Y} {props[i].postScale.Z}");
                node.SetInverseBindPoseMatrix(newTfmSet.GetInverseMatrix4x4());
                aquaNode.nodeList[i] = node;
                newBoneList.Add(newTfmSet);
            }
            foreach (var vtxl in bodyModel.vtxlList)
            {
                List<uint> bonePalette;
                if (bodyModel.bonePalette != null && bodyModel.bonePalette.Count > 0)
                {
                    bonePalette = bodyModel.bonePalette;
                }
                else
                {
                    bonePalette = new List<uint>();
                    for (int i = 0; i < vtxl.bonePalette.Count; i++)
                    {
                        bonePalette.Add(vtxl.bonePalette[i]);
                    }
                }
                for (int i = 0; i < vtxl.vertPositions.Count; i++)
                {
                    Vector3 vertPos = new Vector3();
                    Vector3 vertNrm = new Vector3();
                    var weightIndices = vtxl.trueVertWeightIndices[i];
                    var weights = vtxl.trueVertWeights[i];


                    for (int w = 0; w < weightIndices.Length; w++)
                    {
                        int finalWeightIndex = (int)bonePalette[weightIndices[w]];
                        var localPos = vtxl.vertPositions[i] - oldBoneList[finalWeightIndex].pos;
                        float weight = 0;
                        switch (w)
                        {
                            case 0:
                                weight = weights.X;
                                break;
                            case 1:
                                weight = weights.Y;
                                break;
                            case 2:
                                weight = weights.Z;
                                break;
                            case 3:
                                weight = weights.W;
                                break;
                        }

                        localPos = Vector3.Transform(localPos, oldBoneList[finalWeightIndex].GetInverseMatrix4x4(true));
                        if (props.Count > finalWeightIndex)
                        {
                            //localPos += props[finalWeightIndex].pos;
                            //localPos *= props[finalWeightIndex].scale;

                            //localPos = Vector3.Transform(localPos, props[finalWeightIndex].rot);
                            //localPos = Vector3.Transform(localPos, props[finalWeightIndex].GetMatrix4x4());

                            localPos *= props[finalWeightIndex].scale;

                            //Add scaling for physics parts etc.
                            var bs1 = props[finalWeightIndex].boneShort1;
                            if ((bs1 & 0x40) < 1 && (bs1 & 0x80) < 1 && (bs1 & 0x100) < 1)
                            {
                                var parRot = Quaternion.CreateFromRotationMatrix(newBoneList[aquaNode.nodeList[finalWeightIndex].parentId].GetMatrix4x4());
                                var localRot = newBoneList[finalWeightIndex].rot * Quaternion.Inverse(parRot);

                                localPos = Vector3.Transform(localPos, Quaternion.Inverse(localRot));
                                localPos = Vector3.Transform(localPos, localRot);
                            } 

                            vertNrm += Vector3.TransformNormal(vtxl.vertNormals[i], props[finalWeightIndex].GetMatrix4x4()) * weight;
                        }
                        else
                        {
                            vertNrm += vtxl.vertNormals[i] * weight;
                        }

                        localPos = Vector3.Transform(localPos, newBoneList[finalWeightIndex].GetMatrix4x4(true));

                        if (props.Count > finalWeightIndex)
                        {
                            //Add scaling for physics parts etc.
                            var bs1 = props[finalWeightIndex].boneShort1;
                            if ((bs1 & 0x40) < 1 && (bs1 & 0x80) < 1 && (bs1 & 0x100) < 1)
                            {
                                var postScale = new Vector3(props[finalWeightIndex].postScale.Y, props[finalWeightIndex].postScale.X, props[finalWeightIndex].postScale.Z);
                                
                                localPos *= postScale;
                            }
                        }

                        localPos += newBoneList[finalWeightIndex].pos;
                        vertPos += localPos * weight;
                    }
                    vtxl.vertPositions[i] = vertPos;
                    vtxl.vertNormals[i] = vertNrm;
                }
            }
        }

        //Multiply rotations by inverse of frame 0 and multiply results by each other for relevant bones and transforms.
        //Multiply scales by each other
        //Subtract frame 0 pos for each, add together
        //Returns a transofmration
        public List<TransformSet> CalcBodyTransforms(AquaMotion props, AquaNode aqn)
        {
            var propTransforms = GetPropTransforms(props);
            List<TransformSet> outTfm = new();

            //Get base matrices
            for (int i = 0; i < aqn.nodeList.Count; i++)
            {
                TransformSet tfm = new();
                outTfm.Add(tfm);
            }

            //Physique
            PropTransforms(outTfm, propTransforms, PhysiqueCenterYMin, PhysiqueCenterYMax, GetNGSPropRatio(xxp.baseFIGR.bodyVerts.X));
            PropTransforms(outTfm, propTransforms, PhysiqueSideXMin, PhysiqueSideXMax, GetNGSPropRatio(xxp.baseFIGR.bodyVerts.Y));
            PropTransforms(outTfm, propTransforms, PhysiqueSideYMin, PhysiqueSideYMax, GetNGSPropRatio(xxp.baseFIGR.bodyVerts.Z));

            //Arms
            PropTransforms(outTfm, propTransforms, ArmsCenterYMin, ArmsCenterYMax, GetNGSPropRatio(xxp.baseFIGR.armVerts.X));
            PropTransforms(outTfm, propTransforms, ArmsSideXMin, ArmsSideXMax, GetNGSPropRatio(xxp.baseFIGR.armVerts.Y));
            PropTransforms(outTfm, propTransforms, ArmsSideYMin, ArmsSideYMax, GetNGSPropRatio(xxp.baseFIGR.armVerts.Z));

            //Legs
            PropTransforms(outTfm, propTransforms, LegsCenterYMin, LegsCenterYMax, GetNGSPropRatio(xxp.baseFIGR.legVerts.X));
            PropTransforms(outTfm, propTransforms, LegsSideXMin, LegsSideXMax, GetNGSPropRatio(xxp.baseFIGR.legVerts.Y));
            PropTransforms(outTfm, propTransforms, LegsSideYMin, LegsSideYMax, GetNGSPropRatio(xxp.baseFIGR.legVerts.Z));

            //Chest
            PropTransforms(outTfm, propTransforms, ChestCenterYMin, ChestCenterYMax, GetNGSPropRatio(xxp.baseFIGR.bustVerts.X));
            PropTransforms(outTfm, propTransforms, ChestSideXMin, ChestSideXMax, GetNGSPropRatio(xxp.baseFIGR.bustVerts.Y));
            PropTransforms(outTfm, propTransforms, ChestSideYMin, ChestSideYMax, GetNGSPropRatio(xxp.baseFIGR.bustVerts.Z));

            //Neck
            PropTransforms(outTfm, propTransforms, NeckCenterYMin, NeckCenterYMax, GetNGSPropRatio(xxp.neckVerts.X));
            PropTransforms(outTfm, propTransforms, NeckSideXMin, NeckSideXMax, GetNGSPropRatio(xxp.neckVerts.Y));
            PropTransforms(outTfm, propTransforms, NeckSideYMin, NeckSideYMax, GetNGSPropRatio(xxp.neckVerts.Z));

            //Waist
            PropTransforms(outTfm, propTransforms, WaistCenterYMin, WaistCenterYMax, GetNGSPropRatio(xxp.waistVerts.X));
            PropTransforms(outTfm, propTransforms, WaistSideXMin, WaistSideXMax, GetNGSPropRatio(xxp.waistVerts.Y));
            PropTransforms(outTfm, propTransforms, WaistSideYMin, WaistSideYMax, GetNGSPropRatio(xxp.waistVerts.Z));

            //NGS only
            if (props.moHeader.endFrame > 80)
            {
                PropTransforms(outTfm, propTransforms, ShoulderSizeMin, ShoulderSizeMax, GetNGSPropRatio(xxp.ngsSLID.shoulderSize));

                PropTransforms(outTfm, propTransforms, HandsCenterYMin, HandsCenterYMax, GetNGSPropRatio(xxp.hands.X));
                PropTransforms(outTfm, propTransforms, HandsSideXMin, HandsSideXMax, GetNGSPropRatio(xxp.hands.Y));
                PropTransforms(outTfm, propTransforms, HandsSideYMin, HandsSideYMax, GetNGSPropRatio(xxp.hands.Z));

                PropTransforms(outTfm, propTransforms, NeckAngleMin, NeckAngleMax, GetNGSPropRatio(xxp.neckAngle));
                PropTransforms(outTfm, propTransforms, VerticalPositionArmsMin, VerticalPositionArmsMax, GetNGSPropRatio(xxp.ngsSLID.shoulderVertical));
                PropTransforms(outTfm, propTransforms, ThighsMin, ThighsMax, GetNGSPropRatio(xxp.ngsSLID.thighsAdjust));
                PropTransforms(outTfm, propTransforms, CalvesMin, CalvesMax, GetNGSPropRatio(xxp.ngsSLID.calvesAdjust));
                PropTransforms(outTfm, propTransforms, ForearmsMin, ForearmsMax, GetNGSPropRatio(xxp.ngsSLID.forearmsAdjust));
                PropTransforms(outTfm, propTransforms, HandThicknessMin, HandThicknessMax, GetNGSPropRatio(xxp.ngsSLID.handThickness));
                PropTransforms(outTfm, propTransforms, FootSizeMin, FootSizeMax, GetNGSPropRatio(xxp.ngsSLID.footSize));

            }

            //Fix special NGS bones
            for (int i = 0; i < aqn.nodeList.Count; i++)
            {
                outTfm[i].boneShort1 = aqn.nodeList[i].boneShort1;
                var bs1 = outTfm[i].boneShort1;
                outTfm[i].boneShort2 = aqn.nodeList[i].boneShort2;

                if ((bs1 & 0x40) < 1 && (bs1 & 0x80) < 1 && (bs1 & 0x100) < 1)
                {
                    var parId = GetNonPhysicsParent(aqn, i);
                    var scale = new Vector3(outTfm[parId].scale.X, outTfm[parId].scale.Y, outTfm[parId].scale.Z);
                    outTfm[i].postScale *= scale;
                }
            }

            return outTfm;
        }

        public int GetNonPhysicsParent(AquaNode aqn, int id)
        {
            int parId = id;
            while(parId >= 0)
            {
                parId = aqn.nodeList[parId].parentId;
                var bs1 = aqn.nodeList[parId].boneShort1;
                if((bs1 & 0x40) > 0 || (bs1 & 0x80) > 0 || (bs1 & 0x100) > 0)
                {
                    break;
                }
            }

            return parId;
        }

        public static float GetNGSPropRatio(int prop)
        {
            double absMin = Math.Abs(MinSliderNGS);
            float ratio = (float)((prop + absMin) / (absMin + MaxSliderNGS));
            return ratio;
        }

        public static void PropTransforms(List<TransformSet> outTfm, Dictionary<int, Dictionary<int, TransformSet>> propDict, int minFrame, int maxFrame, float ratio)
        {
            var commonKeys = propDict[maxFrame].Keys.ToList();
            var minKeys = propDict[minFrame].Keys.ToList();
            var maxFrame0List = new List<int>(); //List of nodes where the max frame should be 0
            var minFrame0List = new List<int>(); //List of nodes where the min frame should be 0
            foreach (var key in commonKeys)
            {
                if (!minKeys.Contains(key))
                {
                    minFrame0List.Add(key);
                }
            }

            //Track keys from the other list if they're not in the max list. If we do find them here, make sure we use frame 0 for them
            foreach (var key in minKeys)
            {
                if (!commonKeys.Contains(key))
                {
                    maxFrame0List.Add(key);
                    commonKeys.Add(key);
                }
            }

            foreach (var key in commonKeys)
            {
                var trueMaxFrame = maxFrame;
                var trueMinFrame = minFrame;
                if (maxFrame0List.Contains(key))
                {
                    trueMaxFrame = 0;
                }
                else if (minFrame0List.Contains(key)) //Since the source is only two lists, we can't have both
                {
                    trueMinFrame = 0;
                }
                outTfm[key] = PropTransform(outTfm[key], propDict[trueMinFrame][key], propDict[trueMaxFrame][key], ratio);
            }
        }

        public static TransformSet PropTransform(TransformSet baseTfm, TransformSet tfmMin, TransformSet tfmMax, float ratio)
        {
            return ApplyTransform(baseTfm, InterpolateTransforms(tfmMin, tfmMax, ratio));
        }

        public static TransformSet ApplyTransform(TransformSet baseTfm, TransformSet appliedTfm)
        {
            TransformSet outTfm = baseTfm;
            outTfm.basePos = appliedTfm.basePos;
            outTfm.baseRot = appliedTfm.baseRot;
            outTfm.pos = baseTfm.pos + appliedTfm.pos;
            outTfm.rot = baseTfm.rot * appliedTfm.rot;
            outTfm.scale = baseTfm.scale * appliedTfm.scale;

            return outTfm;
        }

        public static TransformSet InterpolateTransforms(TransformSet tfmMin, TransformSet tfmMax, float ratio)
        {
            TransformSet outTfm = new();
            outTfm.basePos = tfmMin.basePos;
            outTfm.baseRot = tfmMin.baseRot;
            outTfm.pos = Vector3.Lerp(tfmMin.pos, tfmMax.pos, (float)ratio);
            outTfm.rot = Quaternion.Slerp(tfmMin.rot, tfmMax.rot, (float)ratio);
            outTfm.scale = Vector3.Lerp(tfmMin.scale, tfmMax.scale, (float)ratio);

            return outTfm;
        }

        //Returns a set of proportion transforms sorted by keyframe, then node number
        //Frame 0 values are removed from the transforms in order to keep just the values needed for transformation
        public static Dictionary<int, Dictionary<int, TransformSet>> GetPropTransforms(AquaMotion props)
        {
            Dictionary<int, Dictionary<int, TransformSet>> propTransforms = new();
            for (int i = 0; i <= props.moHeader.endFrame; i++)
            {
                propTransforms.Add(i, new Dictionary<int, TransformSet>());
            }

            for (int i = 0; i < props.motionKeys.Count; i++)
            {
                foreach (var data in props.motionKeys[i].keyData)
                {
                    if (data.vector4Keys.Count > 1)
                    {
                        for (int j = 0; j < data.vector4Keys.Count; j++)
                        {
                            int frame = (int)(data.frameTimings[j] / 0x10);
                            TransformSet tfm;
                            if (!propTransforms[frame].ContainsKey(i))
                            {
                                tfm = new TransformSet();

                                //Because this is a proportion file, we can expect these to always be here
                                var pos = props.motionKeys[i].keyData[0].vector4Keys[0];
                                var rot = props.motionKeys[i].keyData[1].vector4Keys[0];
                                var scl = props.motionKeys[i].keyData[2].vector4Keys[0];
                                tfm.basePos = new Vector3(pos.X, pos.Y, pos.Z);
                                tfm.baseRot = new Quaternion(rot.X, rot.Y, rot.Z, rot.W);
                                tfm.pos = new Vector3(0, 0, 0);
                                tfm.rot = Quaternion.Identity;
                                tfm.scale = new Vector3(scl.X, scl.Y, scl.Z);
                            }
                            else
                            {
                                tfm = propTransforms[frame][i];
                            }

                            switch (data.keyType)
                            {
                                case 1: //Pos
                                    var posRaw = data.vector4Keys[j];
                                    if (j != 0) //Remove default local influence
                                    {
                                        var pos = new Vector3(posRaw.X, posRaw.Y, posRaw.Z);
                                        var pos0Raw = data.vector4Keys[0];
                                        var pos0 = new Vector3(pos0Raw.X, pos0Raw.Y, pos0Raw.Z);
                                        var relPos = pos - pos0;
                                        tfm.pos = tfm.pos + relPos;
                                    }
                                    else
                                    {
                                        tfm.pos = new Vector3();
                                    }
                                    break;
                                case 2: //Rot
                                    var rot = data.vector4Keys[j];
                                    if (j != 0) //Remove default local influence
                                    {
                                        var quat = new Quaternion(rot.X, rot.Y, rot.Z, rot.W);
                                        var rot0 = data.vector4Keys[0];
                                        var rot0Quat = new Quaternion(rot0.X, rot0.Y, rot0.Z, rot0.W);
                                        var relQuat = quat * Quaternion.Inverse(rot0Quat);
                                        tfm.rot = tfm.rot * relQuat;
                                    }
                                    else
                                    {
                                        tfm.rot = Quaternion.Identity;
                                    }
                                    tfm.rot = Quaternion.Identity;

                                    break;
                                case 3: //Scale
                                    var scaleRaw = data.vector4Keys[j];
                                    if (j != 0) //Remove default local influence
                                    {
                                        var scale = new Vector3(scaleRaw.X, scaleRaw.Y, scaleRaw.Z);
                                        var scale0Raw = data.vector4Keys[0];
                                        var scale0 = new Vector3(scale0Raw.X, scale0Raw.Y, scale0Raw.Z);
                                        tfm.scale = tfm.scale * (scale / scale0);
                                    }
                                    else
                                    {
                                        tfm.scale = new Vector3(1, 1, 1);
                                    }
                                    break;
                                default:
                                    Trace.WriteLine($"Unexpected keytype {data.keyType}");
                                    break;
                            }
                            propTransforms[frame][i] = tfm;
                        }
                    }
                }
            }

            return propTransforms;
        }

        public static Dictionary<int, Dictionary<int, TransformSet>> GetPropTransformsRaw(AquaMotion props)
        {
            Dictionary<int, Dictionary<int, TransformSet>> propTransforms = new();
            for (int i = 0; i <= props.moHeader.endFrame; i++)
            {
                propTransforms.Add(i, new Dictionary<int, TransformSet>());
            }

            for (int i = 0; i < props.motionKeys.Count; i++)
            {
                foreach (var data in props.motionKeys[i].keyData)
                {
                    if (data.vector4Keys.Count > 1)
                    {
                        for (int j = 0; j < data.vector4Keys.Count; j++)
                        {
                            int frame = (int)(data.frameTimings[j] / 0x10);
                            TransformSet tfm;
                            if (!propTransforms[frame].ContainsKey(i))
                            {
                                tfm = new TransformSet();

                                //Because this is a proportion file, we can expect these to always be here
                                var pos = props.motionKeys[i].keyData[0].vector4Keys[0];
                                var rot = props.motionKeys[i].keyData[1].vector4Keys[0];
                                var scl = props.motionKeys[i].keyData[2].vector4Keys[0];

                                tfm.pos = new Vector3(0, 0, 0);
                                tfm.rot = Quaternion.Identity;
                                tfm.scale = new Vector3(scl.X, scl.Y, scl.Z);
                            }
                            else
                            {
                                tfm = propTransforms[frame][i];
                            }

                            switch (data.keyType)
                            {
                                case 1: //Pos
                                    var posRaw = data.vector4Keys[j];
                                    if (j != 0) //Remove default local influence
                                    {
                                        var pos = new Vector3(posRaw.X, posRaw.Y, posRaw.Z);
                                        var pos0Raw = data.vector4Keys[0];
                                        var pos0 = new Vector3(pos0Raw.X, pos0Raw.Y, pos0Raw.Z);
                                        var relPos = pos - pos0;
                                        tfm.pos = tfm.pos + relPos;
                                    }
                                    else
                                    {
                                        tfm.pos = new Vector3();
                                    }
                                    break;
                                case 2: //Rot
                                    var rot = data.vector4Keys[j];
                                    if (j != 0) //Remove default local influence
                                    {
                                        var quat = new Quaternion(rot.X, rot.Y, rot.Z, rot.W);
                                        var rot0 = data.vector4Keys[0];
                                        var rot0Quat = new Quaternion(rot0.X, rot0.Y, rot0.Z, rot0.W);
                                        var relQuat = quat * Quaternion.Inverse(rot0Quat);
                                        tfm.rot = tfm.rot * relQuat;
                                    }
                                    else
                                    {
                                        tfm.rot = Quaternion.Identity;
                                    }
                                    tfm.rot = Quaternion.Identity;

                                    break;
                                case 3: //Scale
                                    var scaleRaw = data.vector4Keys[j];
                                    if (j != 0) //Remove default local influence
                                    {
                                        var scale = new Vector3(scaleRaw.X, scaleRaw.Y, scaleRaw.Z);
                                        var scale0Raw = data.vector4Keys[0];
                                        var scale0 = new Vector3(scale0Raw.X, scale0Raw.Y, scale0Raw.Z);
                                        tfm.scale = tfm.scale * (scale / scale0);
                                    }
                                    else
                                    {
                                        tfm.scale = new Vector3(1, 1, 1);
                                    }
                                    break;
                                default:
                                    Trace.WriteLine($"Unexpected keytype {data.keyType}");
                                    break;
                            }
                            propTransforms[frame][i] = tfm;
                        }
                    }
                }
            }

            return propTransforms;
        }

        public void GetCharacterFilenames()
        {
            GetBasewearFilenames();
        }

        public void GetBasewearFilenames()
        {
            int id = (int)xxp.baseSLCT2.basewearPart;
            //Get SoundID
            int soundId = -1;
            if (cmx.baseWearDict.ContainsKey(id))
            {
                soundId = cmx.baseWearDict[id].body2.costumeSoundId;
            }

            //Double check these ids and use an adjustedId if needed
            int adjustedId = id;
            if (cmx.baseWearIdLink.ContainsKey(id))
            {
                adjustedId = cmx.baseWearIdLink[id].bcln.fileId;
            }

            //Decide if it needs to be handled as a reboot file or not
            if (id >= 100000)
            {
                string reb = $"{rebootStart}bw_{adjustedId}.ice";
                string rebEx = $"{rebootExStart}bw_{adjustedId}_ex.ice";
                string rebHash = GetFileHash(reb);
                string rebExHash = GetFileHash(rebEx);
                string rebLinkedInner = $"{rebootStart}b1_{adjustedId + 50000}.ice";
                string rebLinkedInnerEx = $"{rebootExStart}b1_{adjustedId + 50000}_ex.ice";
                string rebLinkedInnerHash = GetFileHash(rebLinkedInner);
                string rebLinkedInnerExHash = GetFileHash(rebLinkedInnerEx);

                GetBasewearExtraFileStrings(rebEx, out string rp, out string hn);

                basewearFilename = rebExHash;
                basewearLinkedInnerFilename = rebLinkedInnerExHash;

                basewearRPFilename = rp;
                basewearHNFilename = hn;
            }
            else
            {
                string finalId = $"{adjustedId:D5}";
                string classic = $"{classicStart}bw_{finalId}.ice";

                var classicHash = GetFileHash(classic);
                GetBasewearExtraFileStrings(classic, out string rp, out string hn);

                basewearFilename = classicHash;
                basewearLinkedInnerFilename = null;

                basewearRPFilename = rp;
                basewearHNFilename = hn;
            }
        }

        //String 0 should be the aqp, string 1 should be the aqn
        private static AquaObject GetModel(xxpGeneralReboot xxp, string icePath, List<string> modelStrings, out AquaNode aqn)
        {
            AquaUtil aqua = new AquaUtil();
            var files = GetFilesFromIceLooseMatch(icePath, modelStrings);
            byte[] aqp = new byte[0];
            byte[] aqnode = new byte[0];
            foreach (var file in files)
            {
                if (file.Key.Contains(modelStrings[0]))
                {
                    aqp = file.Value;
                }
                else if (file.Key.Contains(modelStrings[1]))
                {
                    aqnode = file.Value;
                }
            }
            aqua.BeginReadModel(aqp);
            AquaObject model = aqua.aquaModels[0].models[0];
            aqua.ReadBones(aqnode);
            aqn = aqua.aquaBones[0];

            return model;
        }

        private static AquaMotion GetMotion(xxpGeneralReboot xxp, string icePath, string motionString)
        {
            AquaUtil aqua = new AquaUtil();
            byte[] foundFile = GetFilesFromIce(icePath, new List<string>() { motionString })[motionString];
            aqua.ReadMotion(foundFile);
            foundFile = null;
            AquaMotion motion = aqua.aquaMotions[0].anims[0];

            return motion;
        }

        private static Dictionary<string, byte[]> GetFilesFromIce(string icePath, List<string> fileNames)
        {
            Dictionary<string, byte[]> foundFiles = new Dictionary<string, byte[]>();
            if (File.Exists(icePath))
            {
                var strm = new MemoryStream(File.ReadAllBytes(icePath));
                var ice = IceFile.LoadIceFile(strm);
                strm.Dispose();

                List<byte[]> files = new List<byte[]>(ice.groupOneFiles);
                files.AddRange(ice.groupTwoFiles);

                //Loop through files to get what we need
                foreach (byte[] file in files)
                {
                    var name = IceFile.getFileName(file).ToLower();
                    if (fileNames.Contains(name))
                    {
                        foundFiles.Add(name, file);
                    }
                }
                files = null;
                ice = null;
            }

            return foundFiles;
        }

        private static Dictionary<string, byte[]> GetFilesFromIceLooseMatch(string icePath, List<string> fileNames)
        {
            Dictionary<string, byte[]> foundFiles = new Dictionary<string, byte[]>();
            if (File.Exists(icePath))
            {
                var strm = new MemoryStream(File.ReadAllBytes(icePath));
                var ice = IceFile.LoadIceFile(strm);
                strm.Dispose();

                List<byte[]> files = new List<byte[]>(ice.groupOneFiles);
                files.AddRange(ice.groupTwoFiles);

                //Loop through files to get what we need
                foreach (byte[] file in files)
                {
                    var name = IceFile.getFileName(file).ToLower();
                    foreach (var str in fileNames)
                    {
                        if (name.Contains(str))
                        {
                            foundFiles.Add(name, file);
                            break;
                        }
                    }
                }
                files = null;
                ice = null;
            }

            return foundFiles;
        }

        //Does NOT account for hand variants on old type characters. May not be needed??
        private static string GetBodyTypeString(xxpGeneralReboot xxp, out string type)
        {
            type = basewearStr;
            uint costumePart = xxp.baseSLCT.costumePart;
            uint basewearPart = xxp.baseSLCT2.basewearPart;
            uint face = xxp.baseSLCT.faceTypePart;
            if (costumePart >= 300000 && costumePart < 500000)      //NGS Cast
            {
                if (costumePart < 400000)
                {
                    return "pl_cmakemot_b_mh_rb.aqm";
                }
                else
                {
                    return "pl_cmakemot_b_fh_rb.aqm";
                }
            }
            else if (costumePart >= 40000 && costumePart < 60000)  //Cast
            {
                type = castStr;
                if (costumePart < 50000)
                {
                    return "pl_cmakemot_b_mc.aqm";
                }
                else
                {
                    return "pl_cmakemot_b_fc.aqm";
                }
            }
            else if (costumePart > 0 && costumePart < 20000) //Costume
            {
                type = costumeStr;
                if (costumePart < 10000)
                {
                    return "pl_cmakemot_b_mh.aqm";
                }
                else
                {
                    return "pl_cmakemot_b_fh.aqm";
                }
            }
            else                                             //Basewear
            {
                if (basewearPart >= 100000) //Reboot
                {
                    if (basewearPart >= 500000 && (basewearPart < 200000 || basewearPart >= 300000)) //Male. Currently 200000-300000 is unused. 400000-500000 is cast reserved and so not used for basewears. 500000 is 'genderless' which uses male reboot
                    {
                        if (face >= 100000)
                        {
                            return "pl_cmakemot_b_mh_rb.aqm";
                        }
                        else
                        {
                            return "pl_cmakemot_b_mh_rb_oldface.aqm";
                        }
                    }
                    else
                    {
                        if (face >= 100000)
                        {
                            return "pl_cmakemot_b_fh_rb.aqm";
                        }
                        else
                        {
                            return "pl_cmakemot_b_fh_rb_oldface.aqm";
                        }
                    }
                }
                else                     //Classic
                {
                    if (costumePart < 30000)
                    {
                        return "pl_cmakemot_b_mh.aqm";
                    }
                    else
                    {
                        return "pl_cmakemot_b_fh.aqm";
                    }
                }
            }

        }
    }
}
