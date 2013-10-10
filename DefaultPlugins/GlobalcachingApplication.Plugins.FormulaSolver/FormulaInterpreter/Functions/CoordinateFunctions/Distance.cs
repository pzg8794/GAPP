﻿using System;
using Gavaghan.Geodesy;

namespace GlobalcachingApplication.Plugins.FormulaSolver.FormulaInterpreter.Functions.CoordinateFunctions
{
    public class Distance: Functor
    {
        public override object Execute(object[] args, ExecutionContext ctx)
        {
            string res = "";
            ArgumentChecker checker = new ArgumentChecker(ctx, this.GetType().Name);
            if (checker.CheckForMinimumArguments(ref args, 2))
            {
                Framework.Data.Location ll1 = Utils.Conversion.StringToLocation(args[0].ToString());
                Framework.Data.Location ll2 = Utils.Conversion.StringToLocation(args[1].ToString());
                if ((ll1 != null) && (ll2 != null))
                {
                    GeodeticMeasurement gm = Utils.Calculus.CalculateDistance(ll1.Lat, ll1.Lon, ll2.Lat, ll2.Lon);
                    res = gm.PointToPointDistance.ToString("0");
                }
            }
            return res;
        }
    }
}