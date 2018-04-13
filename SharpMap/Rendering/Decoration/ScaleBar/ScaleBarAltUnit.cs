using System;

namespace SharpMap.Rendering.Decoration.ScaleBar
{
    /// <summary>
    /// Scale Bar map decoration with switching between units for large and small scales
    /// </summary>
    [Serializable]
    public class ScaleBarAltUnit : ScaleBar
    {
        //NB defaults set during base class initialisation
        private int _barUnitAlt1;
        private int _barUnitAlt2;

        private double _barUnitFactorAlt1;
        private double _barUnitFactorAlt2;

        private Boolean _forceIt = false;

        /// <summary>
        /// ScaleBar unit. Changing BarUnit will reset BarUnitAlt1 and BarUnitAlt2 accordingly.
        /// </summary>
        public override int BarUnit
        {
            get => base.BarUnit;
            set
            {
                base.BarUnit = value;
                // This effectively reverts to standard ScaleBar behaviour.
                _barUnitAlt1 = BarUnit;
                _barUnitFactorAlt1 = _barUnitFactor;
                _barUnitAlt2 = BarUnit;
                _barUnitFactorAlt2 = _barUnitFactor;
            }
        }

        /// <summary>
        /// Bar Unit for use at large scales (small area) such as ft, m, or yd
        /// </summary>
        public int BarUnitAlt1
        {
            get { return _barUnitAlt1; }
            set
            {
                GetUnitInformation(value, out double d, out string s1, out string s2);

                if (d > 1 || value == (int)Unit.Degree)
                    throw new ArgumentOutOfRangeException("BarUnitAlt1 must be the smaller unit such as ft, m, or yd");

                _barUnitAlt1 = value;
                _barUnitFactorAlt1 = d;
                _forceIt = true;
                OnViewChanged();
                Dirty = true;
            }
        }
        /// <summary>
        /// Bar Unit for use at small scales (large area) such as km, mile, NM
        /// </summary>
        public int BarUnitAlt2
        {
            get { return _barUnitAlt2; }
            set
            {
                GetUnitInformation(value, out double d, out string s1, out string s2);

                if (d < 1 && value != (int)Unit.Degree)
                    throw new ArgumentOutOfRangeException("BarUnitAlt2 must be larger unit such as such as km, mi, nm, or deg");

                _barUnitAlt2 = value;
                _barUnitFactorAlt2 = d;
                _forceIt = true;
                OnViewChanged();
                Dirty = true;
            }
        }

        /// <summary>
        /// Calculate scale bar intervals, plugging BarUnitAlt1 or BarUnitAlt2 into base class BarUnit if change is required
        /// </summary>
        protected override void CalcBarScale(int dpi, int widthOnDevice, int numTics, double mapScale, double fBarUnitFactor, out int pixelsPerTic, out double scaleBarUnitsPerTic)
        {
            // if user has changed BarUnitAlt1 or BarUnitAlt2 then forcefully change base BarUnit
            if (_forceIt)
                base.BarUnit = BarUnitAlt1;

            // now calc intervals for current BarUnitAltX
            base.CalcBarScale(dpi, widthOnDevice, numTics, mapScale, _barUnitFactor, out int pixelsPerTicAlt, out double scaleBarUnitsPerTicAlt);
            // prelim values
            pixelsPerTic = pixelsPerTicAlt;
            scaleBarUnitsPerTic = scaleBarUnitsPerTicAlt;

            // check if need changing
            if (_forceIt || _barUnitAlt1 != _barUnitFactorAlt2)
            {
                if (base._barUnitFactor != _barUnitFactorAlt1)
                {
                    // calculate ScaleBarUnitsPerTic for SMALLER unit
                    base.CalcBarScale(dpi, widthOnDevice, numTics, mapScale, _barUnitFactorAlt1, out int pixelsPerTicSmaller, out double scaleBarUnitsPerTicSmaller);

                    // Switch from LARGER unit to SMALLER unit when LESS than 1 larger unit
                    if (NumTicks * scaleBarUnitsPerTicSmaller * _barUnitFactorAlt1 < _barUnitFactorAlt2)
                    {
                        base.BarUnit = _barUnitAlt1;
                        // using calcs for SMALLER unit
                        pixelsPerTic = pixelsPerTicSmaller;
                        scaleBarUnitsPerTic = scaleBarUnitsPerTicSmaller;
                    }
                }
                else
                {
                    // Switch to LARGER unit when >= 1 larger unit (ie smaller units never exceed 1 larger unit)
                    // scaleBarUnitsPerTicAlt is the SMALLER unit
                    if (NumTicks * scaleBarUnitsPerTicAlt * _barUnitFactorAlt1 >= _barUnitFactorAlt2)
                    {
                        base.BarUnit = _barUnitAlt2;
                        // recalc for LARGER unit
                        base.CalcBarScale(dpi, widthOnDevice, numTics, mapScale, _barUnitFactorAlt2, out pixelsPerTicAlt, out scaleBarUnitsPerTicAlt);
                        pixelsPerTic = pixelsPerTicAlt;
                        scaleBarUnitsPerTic = scaleBarUnitsPerTicAlt;
                    }
                }
            }
            _forceIt = false;
        }
    }
}
