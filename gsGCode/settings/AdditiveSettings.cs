﻿﻿using System;
using g3;
using gs.interfaces;
using Newtonsoft.Json;

namespace gs
{
    public enum MachineClass
    {
        Unknown,
        PlasticFFFPrinter,
        MetalSLSPrinter
    }


    public abstract class MachineInfo : Settings
    {
        protected static string UnknownUUID = "00000000-0000-0000-0000-000000000000";

        public string ManufacturerName = "Unknown";
        public string ManufacturerUUID = UnknownUUID;
        public string ModelIdentifier = "Machine";
        public string ModelUUID = UnknownUUID;
        public MachineClass Class = MachineClass.Unknown;

        public double BedSizeXMM = 100;
        public double BedSizeYMM = 100;
        public double MaxHeightMM = 100;

        // These factors define the output coordinate system

        // BedOriginFactorX:
        // 0   : the origin is at the left of the build plate
        // 0.5 : the origin is at the middle of the build plate
        // 1   : the origin is at the right of the build plate

        // BedOriginFactorY:
        // 0   : the origin is at the front of the build plate
        // 0.5 : the origin is at the middle of the build plate
        // 1   : the origin is at the back of the build plate

        public double BedOriginFactorX = 0;
        public double BedOriginFactorY = 0;
    }


    public class FFFMachineInfo : MachineInfo
    {
        /*
         * printer mechanics
         */
        public double NozzleDiamMM = 0.4;
        public double FilamentDiamMM = 1.75;

        public double MinLayerHeightMM = 0.2;
        public double MaxLayerHeightMM = 0.2;

        /*
         * Temperatures
         */

        public int MinExtruderTempC = 20;
        public int MaxExtruderTempC = 230;

        public bool HasHeatedBed = false;
        public int MinBedTempC = 0;
        public int MaxBedTempC = 0;

        /*
         * All units are mm/min = (mm/s * 60)
         */
        public int MaxExtrudeSpeedMMM = 50 * 60;
        public int MaxTravelSpeedMMM = 100 * 60;
        public int MaxZTravelSpeedMMM = 20 * 60;
        public int MaxRetractSpeedMMM = 20 * 60;


        /*
         *  bed levelling
         */
        public bool HasAutoBedLeveling = false;
        public bool EnableAutoBedLeveling = false;


        /*
         * Hacks?
         */

        public double MinPointSpacingMM = 0.1;          // Avoid emitting gcode extrusion points closer than this spacing.
                                                        // This is a workaround for the fact that many printers do not gracefully
                                                        // handle very tiny sequential extrusion steps. This setting could be
                                                        // configured using CalibrationModelGenerator.MakePrintStepSizeTest() with
                                                        // all other cleanup steps disabled.
                                                        // [TODO] this is actually speed-dependent...
    }


    public interface IPlanarAdditiveSettings 
    {
        double LayerHeightMM { get; }
        AssemblerFactoryF AssemblerType();
    }


    public abstract class PlanarAdditiveSettings : Settings, IPlanarAdditiveSettings
	{
        /// <summary>
        /// This is the "name" of this settings (eg user identifier)
        /// </summary>
        public string Identifier = "Defaults";

		public double LayerHeightMM { get; set; } = 0.2;

        public abstract MachineInfo BaseMachine { get; set; }

        public abstract AssemblerFactoryF AssemblerType();
    }



    public class SingleMaterialFFFSettings : PlanarAdditiveSettings, IProfile
	{
        // This is a bit of an odd place for this, but settings are where we actually
        // know what assembler we should be using...
        public override AssemblerFactoryF AssemblerType()
        {
            throw new NotImplementedException($"{GetType()}.AssemblerType() not provided");
        }

        protected FFFMachineInfo machineInfo;
        public FFFMachineInfo Machine {
            get { if (machineInfo == null) machineInfo = new FFFMachineInfo(); return machineInfo; }
            set { machineInfo = value; }
        }


        public override MachineInfo BaseMachine {
            get { return Machine; }
            set { if (value is FFFMachineInfo)
                    machineInfo = value as FFFMachineInfo;
                 else
                    throw new Exception("SingleMaterialFFFSettings.Machine.set: type is not FFFMachineInfo!");
            }
        }

        #region Material
        public string MaterialSource { get; set; } = "Generic";
        public string MaterialType { get; set; } = "PLA";
        public string MaterialColor { get; set; } = "Blue";

        public string MaterialName => $"{MaterialSource} {MaterialType} - {MaterialColor}";
        #endregion

        /*
         * Temperatures
         */

        public int ExtruderTempC = 210;
        public int HeatedBedTempC = 0;

        /*
		 * Distances.
		 * All units are mm
		 */

        public bool EnableRetraction = true;
		public double RetractDistanceMM = 1.3;
        public double MinRetractTravelLength = 2.5;     // don't retract if we are travelling less than this distance


		/*
		 * Speeds. 
		 * All units are mm/min = (mm/s * 60)
		 */

		// these are all in units of millimeters/minute
		public double RetractSpeed = 25 * 60;   // 1500

		public double ZTravelSpeed = 23 * 60;   // 1380

		public double RapidTravelSpeed = 150 * 60;  // 9000

		public double CarefulExtrudeSpeed = 30 * 60;  	// 1800
		public double RapidExtrudeSpeed = 90 * 60;      // 5400
        public double MinExtrudeSpeed = 20 * 60;        // 600

		public double OuterPerimeterSpeedX = 0.5;

        public double FanSpeedX = 1.0;                  // default fan speed, fraction of max speed (generally unknown)


        // Settings for z-lift on rapid travel moves 
        public bool TravelLiftEnabled { get; set; } = true;
        public double TravelLiftHeight { get; set; } = 0.2;
        public double TravelLiftDistanceThreshold { get; set; } = 5d;

        // Wrap some properties to satisfy the IProfile interface 
        public string ManufacturerName { get => Machine.ManufacturerName; set => Machine.ManufacturerName = value; }
        public string ModelIdentifier { get => Machine.ModelIdentifier; set => Machine.ModelIdentifier = value; }
        public string ProfileName { get => Identifier; set => Identifier = value; }
        public double MachineBedSizeXMM => Machine.BedSizeXMM;
        public double MachineBedSizeYMM => Machine.BedSizeYMM;
        public double MachineBedSizeZMM => Machine.MaxHeightMM;
        public double MachineBedOriginFactorX => Machine.BedOriginFactorX;
        public double MachineBedOriginFactorY => Machine.BedOriginFactorY;
        public virtual IProfile Clone()
        {
            return CloneAs<SingleMaterialFFFSettings>();
        }

        /*
         * Shells
         */
        public int Shells { get; set; } = 2;
        public int InteriorSolidRegionShells = 0;       // how many shells to add around interior solid regions (eg roof/floor)
		public bool OuterShellLast = false;				// do outer shell last (better quality but worse precision)

		/*
		 * Roof/Floors
		 */
		public int RoofLayers { get; set; } = 2;
		public int FloorLayers { get; set; } = 2;

        /*
         *  Solid fill settings
         */
        public double ShellsFillNozzleDiamStepX = 1.0;      // multipler on Machine.NozzleDiamMM, defines spacing between adjacent
                                                            // nested shells/perimeters. If < 1, they overlap.
        public double SolidFillNozzleDiamStepX = 1.0;       // multipler on Machine.NozzleDiamMM, defines spacing between adjacent
                                                            // solid fill parallel lines. If < 1, they overlap.
        public double SolidFillBorderOverlapX = 0.25f;      // this is a multiplier on Machine.NozzleDiamMM, defines how far we
                                                            // overlap solid fill onto border shells (if 0, no overlap)

        /*
		 * Sparse infill settings
		 */
        public double SparseLinearInfillStepX = 5.0;      // this is a multiplier on FillPathSpacingMM

        public double SparseFillBorderOverlapX = 0.25f;     // this is a multiplier on Machine.NozzleDiamMM, defines how far we
                                                            // overlap solid fill onto border shells (if 0, no overlap)

        /*
         * Start layer controls
         */
        public int StartLayers = 0;                      // number of start layers, special handling
        public double StartLayerHeightMM = 0;            // height of start layers. If 0, same as regular layers


        /*
         * Support settings
         */
        public bool GenerateSupport = true;              // should we auto-generate support
        public double SupportOverhangAngleDeg = 35;      // standard "support angle"
        public double SupportSpacingStepX = 5.0;         // usage depends on support technique?           
        public double SupportVolumeScale = 1.0;          // multiplier on extrusion volume
        public bool EnableSupportShell = true;           // should we print a shell around support areas
        public double SupportAreaOffsetX = -0.5;         // 2D inset/outset added to support regions. Multiplier on Machine.NozzleDiamMM.
        public double SupportSolidSpace = 0.35f;         // how much space to leave between model and support
		public double SupportRegionJoinTolX = 2.0;		 // support regions within this distance will be merged via topological dilation. Multiplier on NozzleDiamMM.
        public bool EnableSupportReleaseOpt = true;      // should we use support release optimization
        public double SupportReleaseGap = 0.2f;          // how much space do we leave
        public double SupportMinDimension = 1.5;         // minimal size of support polygons
        public bool SupportMinZTips = true;              // turn on/off detection of support 'tip' regions, ie tiny islands.
		public double SupportPointDiam = 2.5f;           // width of per-layer support "points" (keep larger than SupportMinDimension!)
		public int SupportPointSides = 4;                // number of vertices for support-point polygons (circles)


		/*
		 * Bridging settings
		 */
		public bool EnableBridging = true;
		public double MaxBridgeWidthMM = 10.0;
		public double BridgeFillNozzleDiamStepX = 0.85;  // multiplier on FillPathSpacingMM
		public double BridgeVolumeScale = 1.0;           // multiplier on extrusion volume
		public double BridgeExtrudeSpeedX = 0.5;		 // multiplier on CarefulExtrudeSpeed


        /*
         * Toolpath filtering options
         */
        public double MinLayerTime = 5.0;                // minimum layer time in seconds
        public bool ClipSelfOverlaps = false;            // if true, try to remove portions of toolpaths that will self-overlap
        public double SelfOverlapToleranceX = 0.75;      // what counts as 'self-overlap'. this is a multiplier on NozzleDiamMM

        /*
         * Debug/Utility options
         */

        [JsonIgnore]
        public Interval1i LayerRangeFilter = new Interval1i(0, 999999999);   // only compute slices in this range

        [JsonProperty]
        private int LayerRangeFilterMin { get { return LayerRangeFilter.a; } set { LayerRangeFilter.a = value; } }
        private int LayerRangeFilterMax { get { return LayerRangeFilter.b; } set { LayerRangeFilter.b = value; } }

        /*
         * functions that calculate derived values
         * NOTE: these cannot be properties because then they will be json-serialized!
         */
        public double ShellsFillPathSpacingMM() {
            return Machine.NozzleDiamMM * ShellsFillNozzleDiamStepX;
        }
        public double SolidFillPathSpacingMM() {
            return Machine.NozzleDiamMM * SolidFillNozzleDiamStepX;
        }
        public double BridgeFillPathSpacingMM() {
			return Machine.NozzleDiamMM * BridgeFillNozzleDiamStepX;
        }
    }




    // just for naming...
    public class GenericRepRapSettings : SingleMaterialFFFSettings
    {
        public override AssemblerFactoryF AssemblerType() {
            return RepRapAssembler.Factory;
        }
    }

}