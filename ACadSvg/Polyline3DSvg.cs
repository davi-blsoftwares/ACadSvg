﻿#region copyright LGPL nanoLogika
//  Copyright 2023, nanoLogika GmbH.
//  All rights reserved. 
//  This source code is licensed under the "LGPL v3 or any later version" license. 
//  See LICENSE file in the project root for full license information.
#endregion

using ACadSharp.Entities;
using SvgElements;


namespace ACadSvg {

    internal class Polyline3DSvg : EntitySvg {

        private Polyline3D _polyline;


		/// <summary>
		/// Initializes a new instance of the <see cref="Polyline3DSvg"/> class
		/// for the specified <see cref="Polyline3D"/> entity.
		/// </summary>
		/// <param name="polyline">The <see cref="Polyline3D"/> entity to be converted.</param>
		/// <param name="ctx">This parameter is not used in this class.</param>
		public Polyline3DSvg(Entity polyline, ConversionContext ctx) {
            _polyline = (Polyline3D)polyline;
			SetStandardIdAndClassIf(polyline, ctx);
		}


		/// <inheritdoc />
		public override SvgElementBase ToSvgElement() {
			var path = new PathElement();
			path.AddPoints(Utils.VerticesToArray(_polyline.Vertices.ToList()));

			if (_polyline.IsClosed) {
				path.Close();
			}

			return path
				.WithID(ID)
				.WithClass(Class)
				.WithStroke(ColorUtils.GetHtmlColor(_polyline, _polyline.Color))
				.WithStrokeDashArray(Utils.LineToDashArray(_polyline, _polyline.LineType));
		}
    }
}