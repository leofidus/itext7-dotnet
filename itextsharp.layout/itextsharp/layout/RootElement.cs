/*
$Id: df51b43bc75f2306a0d615b14b1ef6c6556c6fff $

This file is part of the iText (R) project.
Copyright (c) 1998-2016 iText Group NV
Authors: Bruno Lowagie, Paulo Soares, et al.

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License version 3
as published by the Free Software Foundation with the addition of the
following permission added to Section 15 as permitted in Section 7(a):
FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
OF THIRD PARTY RIGHTS

This program is distributed in the hope that it will be useful, but
WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
or FITNESS FOR A PARTICULAR PURPOSE.
See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License
along with this program; if not, see http://www.gnu.org/licenses or write to
the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
Boston, MA, 02110-1301 USA, or download the license from the following URL:
http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions
of this program must display Appropriate Legal Notices, as required under
Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License,
a covered work must retain the producer line in every PDF that is created
or manipulated using iText.

You can be released from the requirements of the license by purchasing
a commercial license. Buying such a license is mandatory as soon as you
develop commercial activities involving the iText software without
disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP,
serving PDFs on the fly in a web application, shipping iText with a closed
source product.

For more information, please contact iText Software Corp. at this
address: sales@itextpdf.com
*/
using System;
using System.Collections.Generic;
using com.itextpdf.kernel.font;
using com.itextpdf.kernel.pdf;
using com.itextpdf.kernel.pdf.canvas;
using com.itextpdf.layout.element;
using com.itextpdf.layout.property;
using com.itextpdf.layout.renderer;
using com.itextpdf.layout.splitting;

namespace com.itextpdf.layout
{
	/// <summary>A generic abstract root element for a PDF layout object hierarchy.</summary>
	/// <?/>
	public abstract class RootElement<T> : ElementPropertyContainer<T>
		where T : IPropertyContainer
	{
		protected internal bool immediateFlush = true;

		protected internal PdfDocument pdfDocument;

		protected internal IList<IElement> childElements = new List<IElement>();

		protected internal IDictionary<Property, Object> properties = new EnumMap<Property
			, Object>(typeof(Property));

		protected internal PdfFont defaultFont;

		protected internal ISplitCharacters defaultSplitCharacters;

		protected internal RootRenderer rootRenderer;

		/// <summary>Adds an element to the root.</summary>
		/// <remarks>Adds an element to the root. The element is immediately placed in the contents.
		/// 	</remarks>
		/// <param name="element">an element with spacial margins, tabbing, and alignment</param>
		/// <returns>this element</returns>
		/// <seealso cref="com.itextpdf.layout.element.BlockElement{T}"/>
		public virtual RootElement<T> Add<T2>(BlockElement<T2> element)
			where T2 : IElement
		{
			childElements.Add(element);
			EnsureRootRendererNotNull().AddChild(element.CreateRendererSubTree());
			return this;
		}

		/// <summary>Adds an image to the root.</summary>
		/// <remarks>Adds an image to the root. The element is immediately placed in the contents.
		/// 	</remarks>
		/// <param name="image">a graphical image element</param>
		/// <returns>this element</returns>
		/// <seealso cref="com.itextpdf.layout.element.Image"/>
		public virtual RootElement<T> Add(Image image)
		{
			childElements.Add(image);
			EnsureRootRendererNotNull().AddChild(image.CreateRendererSubTree());
			return this;
		}

		public override bool HasProperty(Property property)
		{
			return HasOwnProperty(property);
		}

		public override bool HasOwnProperty(Property property)
		{
			return properties.ContainsKey(property);
		}

		public override T1 GetProperty<T1>(Property property)
		{
			return ((T1)GetOwnProperty(property));
		}

		public override T1 GetOwnProperty<T1>(Property property)
		{
			return (T1)properties[property];
		}

		public override T1 GetDefaultProperty<T1>(Property property)
		{
			try
			{
				switch (property)
				{
					case Property.FONT:
					{
						if (defaultFont == null)
						{
							defaultFont = PdfFontFactory.CreateFont();
						}
						return (T1)defaultFont;
					}

					case Property.SPLIT_CHARACTERS:
					{
						if (defaultSplitCharacters == null)
						{
							defaultSplitCharacters = new DefaultSplitCharacters();
						}
						return (T1)defaultSplitCharacters;
					}

					case Property.FONT_SIZE:
					{
						return (T1)System.Convert.ToInt32(12);
					}

					case Property.TEXT_RENDERING_MODE:
					{
						return (T1)System.Convert.ToInt32(PdfCanvasConstants.TextRenderingMode.FILL);
					}

					case Property.TEXT_RISE:
					{
						return (T1)float.ValueOf(0);
					}

					case Property.SPACING_RATIO:
					{
						return (T1)float.ValueOf(0.75f);
					}

					case Property.FONT_KERNING:
					{
						return (T1)FontKerning.NO;
					}

					case Property.BASE_DIRECTION:
					{
						return (T1)BaseDirection.NO_BIDI;
					}

					default:
					{
						return null;
					}
				}
			}
			catch (System.IO.IOException exc)
			{
				throw new Exception(exc);
			}
		}

		public override void DeleteOwnProperty(Property property)
		{
			properties.Remove(property);
		}

		public override void SetProperty(Property property, Object value)
		{
			properties[property] = value;
		}

		/// <summary>
		/// Gets the rootRenderer attribute, a specialized
		/// <see cref="com.itextpdf.layout.renderer.IRenderer"/>
		/// that
		/// acts as the root object that other
		/// <see cref="com.itextpdf.layout.renderer.IRenderer">renderers</see>
		/// descend
		/// from.
		/// </summary>
		/// <returns>
		/// the
		/// <see cref="com.itextpdf.layout.renderer.RootRenderer"/>
		/// attribute
		/// </returns>
		public virtual RootRenderer GetRenderer()
		{
			return EnsureRootRendererNotNull();
		}

		/// <summary>Convenience method to write a text aligned about the specified point</summary>
		/// <param name="text">text to be placed to the page</param>
		/// <param name="x">the point about which the text will be aligned and rotated</param>
		/// <param name="y">the point about which the text will be aligned and rotated</param>
		/// <param name="textAlign">horizontal alignment about the specified point</param>
		/// <returns>this object</returns>
		public virtual RootElement<T> ShowTextAligned(String text, float x, float y, TextAlignment
			 textAlign)
		{
			return ShowTextAligned(text, x, y, textAlign, 0);
		}

		/// <summary>Convenience method to write a text aligned about the specified point</summary>
		/// <param name="text">text to be placed to the page</param>
		/// <param name="x">the point about which the text will be aligned and rotated</param>
		/// <param name="y">the point about which the text will be aligned and rotated</param>
		/// <param name="textAlign">horizontal alignment about the specified point</param>
		/// <param name="angle">the angle of rotation applied to the text, in radians</param>
		/// <returns>this object</returns>
		public virtual RootElement<T> ShowTextAligned(String text, float x, float y, TextAlignment
			 textAlign, float angle)
		{
			return ShowTextAligned(text, x, y, textAlign, VerticalAlignment.BOTTOM, angle);
		}

		/// <summary>Convenience method to write a text aligned about the specified point</summary>
		/// <param name="text">text to be placed to the page</param>
		/// <param name="x">the point about which the text will be aligned and rotated</param>
		/// <param name="y">the point about which the text will be aligned and rotated</param>
		/// <param name="textAlign">horizontal alignment about the specified point</param>
		/// <param name="vertAlign">vertical alignment about the specified point</param>
		/// <param name="angle">the angle of rotation applied to the text, in radians</param>
		/// <returns>this object</returns>
		public virtual RootElement<T> ShowTextAligned(String text, float x, float y, TextAlignment
			 textAlign, VerticalAlignment vertAlign, float angle)
		{
			Paragraph p = new Paragraph(text);
			return ShowTextAligned(p, x, y, pdfDocument.GetNumberOfPages(), textAlign, vertAlign
				, angle);
		}

		/// <summary>Convenience method to write a kerned text aligned about the specified point
		/// 	</summary>
		/// <param name="text">text to be placed to the page</param>
		/// <param name="x">the point about which the text will be aligned and rotated</param>
		/// <param name="y">the point about which the text will be aligned and rotated</param>
		/// <param name="textAlign">horizontal alignment about the specified point</param>
		/// <param name="vertAlign">vertical alignment about the specified point</param>
		/// <param name="angle">the angle of rotation applied to the text, in radians</param>
		/// <returns>this object</returns>
		public virtual RootElement<T> ShowTextAlignedKerned(String text, float x, float y
			, TextAlignment textAlign, VerticalAlignment vertAlign, float angle)
		{
			Paragraph p = new Paragraph(text).SetFontKerning(FontKerning.YES);
			return ShowTextAligned(p, x, y, pdfDocument.GetNumberOfPages(), textAlign, vertAlign
				, angle);
		}

		/// <summary>Convenience method to write a text aligned about the specified point</summary>
		/// <param name="p">
		/// paragraph of text to be placed to the page. By default it has no leading and is written in single line.
		/// Set width to write multiline text.
		/// </param>
		/// <param name="x">the point about which the text will be aligned and rotated</param>
		/// <param name="y">the point about which the text will be aligned and rotated</param>
		/// <param name="textAlign">horizontal alignment about the specified point</param>
		/// <returns>this object</returns>
		public virtual RootElement<T> ShowTextAligned(Paragraph p, float x, float y, TextAlignment
			 textAlign)
		{
			return ShowTextAligned(p, x, y, pdfDocument.GetNumberOfPages(), textAlign, VerticalAlignment
				.BOTTOM, 0);
		}

		/// <summary>Convenience method to write a text aligned about the specified point</summary>
		/// <param name="p">
		/// paragraph of text to be placed to the page. By default it has no leading and is written in single line.
		/// Set width to write multiline text.
		/// </param>
		/// <param name="x">the point about which the text will be aligned and rotated</param>
		/// <param name="y">the point about which the text will be aligned and rotated</param>
		/// <param name="textAlign">horizontal alignment about the specified point</param>
		/// <param name="vertAlign">vertical alignment about the specified point</param>
		/// <returns>this object</returns>
		public virtual RootElement<T> ShowTextAligned(Paragraph p, float x, float y, TextAlignment
			 textAlign, VerticalAlignment vertAlign)
		{
			return ShowTextAligned(p, x, y, pdfDocument.GetNumberOfPages(), textAlign, vertAlign
				, 0);
		}

		/// <summary>Convenience method to write a text aligned about the specified point</summary>
		/// <param name="p">
		/// paragraph of text to be placed to the page. By default it has no leading and is written in single line.
		/// Set width to write multiline text.
		/// </param>
		/// <param name="x">the point about which the text will be aligned and rotated</param>
		/// <param name="y">the point about which the text will be aligned and rotated</param>
		/// <param name="pageNumber">the page number to write the text</param>
		/// <param name="textAlign">horizontal alignment about the specified point</param>
		/// <param name="vertAlign">vertical alignment about the specified point</param>
		/// <param name="angle">the angle of rotation applied to the text, in radians</param>
		/// <returns>this object</returns>
		public virtual RootElement<T> ShowTextAligned(Paragraph p, float x, float y, int 
			pageNumber, TextAlignment textAlign, VerticalAlignment vertAlign, float angle)
		{
			Div div = new Div();
			div.SetTextAlignment(textAlign).SetVerticalAlignment(vertAlign);
			if (angle != 0)
			{
				div.SetRotationAngle(angle);
			}
			div.SetProperty(Property.ROTATION_POINT_X, x);
			div.SetProperty(Property.ROTATION_POINT_Y, y);
			float divWidth = AbstractRenderer.INF;
			float divHeight = AbstractRenderer.INF;
			float divX = x;
			float divY = y;
			if (textAlign == TextAlignment.CENTER)
			{
				divX = x - divWidth / 2;
				p.SetHorizontalAlignment(HorizontalAlignment.CENTER);
			}
			else
			{
				if (textAlign == TextAlignment.RIGHT)
				{
					divX = x - divWidth;
					p.SetHorizontalAlignment(HorizontalAlignment.RIGHT);
				}
			}
			if (vertAlign == VerticalAlignment.MIDDLE)
			{
				divY = y - divHeight / 2;
			}
			else
			{
				if (vertAlign == VerticalAlignment.TOP)
				{
					divY = y - divHeight;
				}
			}
			if (pageNumber == 0)
			{
				pageNumber = 1;
			}
			div.SetFixedPosition(pageNumber, divX, divY, divWidth).SetHeight(divHeight);
			if (((Object)p.GetProperty(Property.LEADING)) == null)
			{
				p.SetMultipliedLeading(1);
			}
			div.Add(p.SetMargins(0, 0, 0, 0));
			div.SetRole(PdfName.Artifact);
			this.Add(div);
			return this;
		}

		protected internal abstract RootRenderer EnsureRootRendererNotNull();
	}
}