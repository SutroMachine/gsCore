﻿using g3;
using System;
using System.Collections.Generic;

namespace gs
{
    public interface IGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="globalSettings"></param>
        /// <param name="gcodeLineReadyF"></param>
        /// <param name="layerReadyF"></param>
        /// <returns></returns>
        GCodeFile GenerateGCode(IList<Tuple<DMesh3, object>> parts,
                                object globalSettings,
                                Action<GCodeLine> gcodeLineReadyF = null,
                                Action<PrintLayerData> layerReadyF = null,
                                Action<string> progressMessageF = null);

        void SaveGCode(string path, GCodeFile file);

        GCodeFile LoadGCode(string path);

        bool AcceptsParts { get; }
        bool AcceptsPartSettings { get; }
    }

    public interface IGenerator<TSettings> : IGenerator
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="globalSettings"></param>
        /// <param name="gcodeLineReadyF"></param>
        /// <param name="layerReadyF"></param>
        /// <returns></returns>
        GCodeFile GenerateGCode(IList<Tuple<DMesh3, TSettings>> parts,
                                TSettings globalSettings,
                                Action<GCodeLine> gcodeLineReadyF = null,
                                Action<PrintLayerData> layerReadyF = null,
                                Action<string> progressMessageF = null);


    }
}