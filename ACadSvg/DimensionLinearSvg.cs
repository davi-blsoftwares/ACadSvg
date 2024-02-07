﻿#region copyright LGPL nanoLogika
//  Copyright 2023, nanoLogika GmbH.
//  All rights reserved. 
//  This source code is licensed under the "LGPL v3 or any later version" license. 
//  See LICENSE file in the project root for full license information.
#endregion

using ACadSharp.Entities;
using ACadSharp.Tables;

using CSMath;

using SvgElements;


namespace ACadSvg {

    /// <summary>
    /// Represents an SVG element converted from an ACad <see cref="DimensionLinear"/> entity.
    /// The <see cref="DimensionLinear"/> entity is a complex element including several <i>path</i>
    /// elements for the dimension lines, extension lines, filled <i>path</i> elements for the
    /// standard arrowheads, and finally a <i>text</i> element for the mesurement text.
    /// </summary>
    internal class DimensionLinearSvg : DimensionSvg {

        private DimensionLinear _linDim;


        /// <summary>
        /// Initializes a new instance of the <see cref="DimensionLinearSvg"/> class
        /// for the specified <see cref="DimensionLinear"/> entity.
        /// </summary>
        /// <param name="linDim">The <see cref="DimensionLinear"/> entity to be converted.</param>
        /// <param name="ctx">This parameter is not used in this class.</param>
        public DimensionLinearSvg(Entity linDim, ConversionContext ctx) : base(linDim, ctx) {
            _linDim = (DimensionLinear)linDim;
            _defaultPostFix = "<>";
        }


        /// <inheritdoc />
        public override SvgElementBase ToSvgElement() {
            CreateGroupElement();

            // Get points for measurement
            XY p1 = Utils.ToXY(_linDim.FirstPoint);
            XY p2 = Utils.ToXY(_linDim.SecondPoint);
            XY dp2 = Utils.ToXY(_linDim.DefinitionPoint);

            double dirp1p2 = (p2 - p1).GetAngle();
            double dirp1dp2 = (dp2 - p1).GetAngle();
            bool ccw = Math.Sin(dirp1dp2 - dirp1p2) > 0;
            XYZ n = XYZ.Cross(_linDim.SecondPoint - _linDim.FirstPoint, _linDim.DefinitionPoint - _linDim.FirstPoint);
            ccw = n.Z > 0;

            //  TODO Find out what these properties are for
            var dimRot = _linDim.Rotation;
            var dimHor = _linDim.HorizontalDirection;
            var extLineRot = _linDim.ExtLineRotation;

            double extLineExt = _dimProps.ExtensionLineExtension;
            double extLineOffset = _dimProps.ExtensionLineOffset;

            XY dext2 = dp2 - p2;
            double dext2el = dext2.GetLength() + extLineExt;
            double dext2al = extLineOffset;
            XY dextn = dext2.Normalize();
            XY dext2e = p2 + dext2el * dextn;
            XY dext2a = p2 + dext2al * dextn;

            XY dimDir = ccw ? new XY(-dextn.Y, dextn.X) : new XY(dextn.Y, -dextn.X);
            XY dp1 = dp2 + dimDir * _linDim.Measurement;
            XY dext1 = dp1 - p1;
            double dext1el = dext1.GetLength() + extLineExt;
            double dext1al = extLineOffset;
            XY dext1n = dext1.Normalize();
            XY dext1e = p1 + dext1el * dext1n;
            XY dext1a = p1 + dext1al * dext1n;

            //  Get arrow´position, direction and color
            GetArrowsOutside(_linDim.Measurement, out bool firstArrowOutside, out bool secondArrowOutside);
            XY arrow1Direction = dimDir * (firstArrowOutside ? -1 : 1);
            XY arrow2Direction = -dimDir * (secondArrowOutside ? -1 : 1);

            //  Get individual arrowheads
            var arr1Block = _dimProps.ArrowHeadBlock1;
            var arr2Block = _dimProps.ArrowHeadBlock2;

            //  Get measurement text
            double textSize = TextUtils.GetTextSize(_dimProps.TextHeight) * 1.5;
            double textRot = (GetTextRot(dimDir.GetAngle()) + _linDim.TextRotation) * 180 / Math.PI;
            const double noTextLen = 0;

            XY dimMid = GetMidpoint(dp1, dp2);
            XY textMid = Utils.ToXY(_linDim.TextMiddlePoint);
            XY textOnDimLin = dp1 + dimDir * dimDir.Dot(textMid - dp1);
            bool withLeader = (textMid - textOnDimLin).GetLength() > textSize * 1.4 && _dimProps.TextMovement == TextMovement.AddLeaderWhenTextMoved;

            double textLen = noTextLen;
            if (withLeader) {
                //  Draw measuring text and leader element
                CreateTextElementAndLeader(dimMid, textMid, dimDir, textRot);
            }
            else {
                //  Draw measuring mext
                XY textPos = textOnDimLin;
                CreateTextElement(textPos, dimDir.GetAngle(), out textLen);
            }

            bool textInside = (dp2 - dp1).GetLength() > (textOnDimLin - dp1).GetLength();

            //  Calculate dimension line length
            double dimensionLineExtension = _dimProps.DimensionLineExtension;
            XY dl1;
            XY dl2;
            if (dimensionLineExtension > 0) {
                dl1 = dp1 + dimDir * dimensionLineExtension;
                dl2 = dp2 - dimDir * dimensionLineExtension;
            }
            else {
                //  Inside dimension line extensions, shortened when arrows are inside
                dl1 = firstArrowOutside ? dp1 : dp1 - dimDir * _arrowSize;
                dl2 = secondArrowOutside ? dp2 : dp2 + dimDir * _arrowSize;
            }

            //  Draw lines
            CreateFirstExtensionLine(dext1a, dext1e);
            CreateSecondExtensionLine(dext2a, dext2e);
            CreateDimensionLine(dl1, dl2);

            //  Draw dimension line extensions
            CreateDimensionLineExtension(dp1, dp1 + dimDir * 2 * _arrowSize, dimDir, _arrowSize, false, firstArrowOutside, noTextLen);
            CreateDimensionLineExtension(dp2, textOnDimLin, -dimDir, _arrowSize, !textInside, secondArrowOutside, textLen);

            //  Draw arrow heads
            CreateArrowHead(arr1Block, dp1, arrow1Direction);
            CreateArrowHead(arr2Block, dp2, arrow2Direction);
         
            //  TODO
            var attachmentPoint = _linDim.AttachmentPoint;
            var verticalAlignment = _dimProps.TextVerticalAlignment;
            var verticalPosition = _dimProps.TextVerticalPosition;

            CreateDebugPoint(textMid, "red");

            //  Return all drawing elements 
            return _groupElement;
        }
    }
}