﻿using System;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace AeroWizard.Design
{
	internal class WizardPageDesigner : RichParentControlDesigner<WizardPage, WizardPageDesigner.WizardPageDesignerActionList>
	{
		private static readonly string[] propsToRemove = new string[] { "Anchor", "AutoScrollOffset", "AutoSize", "BackColor",
			"BackgroundImage", "BackgroundImageLayout", "ContextMenuStrip", "Cursor", "Dock", "Enabled", "Font",
			"ForeColor", "Location", "Margin", "MaximumSize", "MinimumSize", "Padding", "TabStop", "UseWaitCursor", "Visible" };

		public override SelectionRules SelectionRules => SelectionRules.Visible | SelectionRules.Locked;

		internal WizardBaseDesigner ContainerDesigner => GetService<IDesignerHost>()?.GetDesigner(Control.Owner) as WizardBaseDesigner;
		protected override bool EnableDragRect => false;

		protected override System.Collections.Generic.IEnumerable<string> PropertiesToRemove => propsToRemove;

		public override bool CanBeParentedTo(IDesigner parentDesigner) => parentDesigner?.Component is WizardPageContainer;

		public override void Initialize(System.ComponentModel.IComponent component)
		{
			base.Initialize(component);
			//base.Glyphs.Add(new WizardPageDesignerGlyph(this));
			GetService<DesignerActionService>()?.Remove(component);
		}

		internal void OnDragDropInternal(DragEventArgs de) => OnDragDrop(de);

		internal void OnDragEnterInternal(DragEventArgs de) => OnDragEnter(de);

		internal void OnDragLeaveInternal(EventArgs e) => OnDragLeave(e);

		internal void OnDragOverInternal(DragEventArgs e) => OnDragOver(e);

		internal void OnGiveFeedbackInternal(GiveFeedbackEventArgs e) => OnGiveFeedback(e);

		protected override void OnPaintAdornments(PaintEventArgs pe)
		{
			Rectangle clientRectangle = Control.ClientRectangle;
			clientRectangle.Width--;
			clientRectangle.Height--;
			ControlPaint.DrawFocusRectangle(pe.Graphics, clientRectangle);
			base.OnPaintAdornments(pe);
		}

		internal class WizardPageDesignerActionList : RichDesignerActionList<WizardPageDesigner, WizardPage>
		{
			public WizardPageDesignerActionList(WizardPageDesigner pageDesigner, WizardPage page)
				: base(pageDesigner, page)
			{
			}

			[DesignerActionProperty("Back Button Enabled", 0, Category = "Buttons", Description = "Enables the Back button when this page is shown.")]
			public bool AllowBack
			{
				get => Component.AllowBack;
				set => Component.AllowBack = value;
			}

			[DesignerActionProperty("Cancel Button Enabled", 1, Category = "Buttons", Description = "Enables the Cancel button when this page is shown.")]
			public bool AllowCancel
			{
				get => Component.AllowCancel;
				set => Component.AllowCancel = value;
			}

			[DesignerActionProperty("Next Button Enabled", 3, Category = "Buttons")]
			public bool AllowNext
			{
				get => Component.AllowNext;
				set => Component.AllowNext = value;
			}

			[DesignerActionProperty("Mark As Finish Page", 5, Category = "Behavior")]
			public bool IsFinishPage
			{
				get => Component.IsFinishPage;
				set => Component.IsFinishPage = value;
			}

			[DesignerActionProperty("Set Next Page", 6, Category = "Behavior")]
			public WizardPage NextPage
			{
				get => Component.NextPage;
				set => Component.NextPage = value;
			}

			[DesignerActionProperty("Cancel Button Visible", 2, Category = "Buttons")]
			public bool ShowCancel
			{
				get => Component.ShowCancel;
				set => Component.ShowCancel = value;
			}

			[DesignerActionProperty("Next Button Visible", 4, Category = "Behavior")]
			public bool ShowNext
			{
				get => Component.ShowNext;
				set => Component.ShowNext = value;
			}
		}
	}

	internal class WizardPageDesignerBehavior : RichBehavior<WizardPageDesigner>
	{
		public WizardPageDesignerBehavior(WizardPageDesigner designer)
			: base(designer)
		{
		}

		public override bool OnMouseDown(System.Windows.Forms.Design.Behavior.Glyph g, MouseButtons button, Point mouseLoc)
		{
			if (button == MouseButtons.Left)
			{
				switch (((WizardPageDesignerGlyph)g).LastHit)
				{
					case WizardPageDesignerGlyph.ClickState.FirstBtn:
						Designer.Control.Owner.SelectedPage = Designer.Control.Owner.Pages[0];
						break;

					case WizardPageDesignerGlyph.ClickState.PrevBtn:
						Designer.Control.Owner.PreviousPage();
						break;

					case WizardPageDesignerGlyph.ClickState.NextBtn:
						Designer.Control.Owner.NextPage();
						break;

					case WizardPageDesignerGlyph.ClickState.LastBtn:
						Designer.Control.Owner.SelectedPage = Designer.Control.Owner.Pages[Designer.Control.Owner.Pages.Count - 1];
						break;

					case WizardPageDesignerGlyph.ClickState.Control:
						break;

					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			return base.OnMouseDown(g, button, mouseLoc);
		}
	}

	internal class WizardPageDesignerGlyph : RichGlyph<WizardPageDesigner>
	{
		private const int btnCount = 4, btnSize = 16, navBoxWidth = btnSize * btnCount + (btnCount - 1) * 2 + 4, navBoxHeight = btnSize + 4;
		private Rectangle navBox;

		public WizardPageDesignerGlyph(WizardPageDesigner designer) : base(designer, new WizardPageDesignerBehavior(designer))
		{
			Designer.SelectionService.SelectionChanged += selSvc_SelectionChanged;
			Designer.Control.Move += control_Move;
			Designer.Control.Resize += control_Move;
		}

		internal enum ClickState
		{
			Control,
			FirstBtn,
			PrevBtn,
			NextBtn,
			LastBtn
		}

		public override Rectangle Bounds => navBox;

		internal ClickState LastHit { get; set; }

		public override void Dispose()
		{
			Designer.SelectionService.SelectionChanged -= selSvc_SelectionChanged;
			Designer.Control.Move -= control_Move;
			Designer.Control.Resize -= control_Move;
			base.Dispose();
		}

		public override Cursor GetHitTest(Point p)
		{
			Rectangle r1 = new(navBox.X + 2, navBox.Y + 2, btnSize, btnSize);
			for (int i = 0; i < btnCount; i++)
			{
				if (r1.Contains(p))
				{
					LastHit = (ClickState)(i + 1);
					return Cursors.Arrow;
				}
				r1.Offset(btnSize + 2, 0);
			}
			LastHit = ClickState.Control;
			return null;
		}

		public override void Paint(PaintEventArgs pe)
		{
			bool isMin7 = Environment.OSVersion.Version >= new Version(6, 1);
			string fn = isMin7 ? "Webdings" : "Arial Narrow";
			string[] btnText = isMin7 ? new[] { "9", "3", "4", ":" } : new[] { "«", "<", ">", "»" };
			using Font f = new(fn, btnSize - 2, isMin7 ? FontStyle.Regular : FontStyle.Bold, GraphicsUnit.Pixel);
			pe.Graphics.FillRectangle(SystemBrushes.Control, new Rectangle(navBox.X, navBox.Y, navBox.Width + 1, navBox.Height + 1));
			using Pen pen = new(SystemBrushes.ControlDark, 1f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot };
			pe.Graphics.DrawRectangle(pen, navBox);
			Rectangle r1 = new(navBox.X + 2, navBox.Y + 2, btnSize, btnSize);
			pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;
			StringFormat sf = new() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
			for (int i = 0; i < btnCount; i++)
			{
				pe.Graphics.DrawRectangle(pen, r1);
				r1.Offset(1, 1);
				//TextRenderer.DrawText(pe.Graphics, btnText[i], f, r1, SystemColors.ControlDark, SystemColors.Window, TextFormatFlags.NoPadding | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
				pe.Graphics.DrawString(btnText[i], f, SystemBrushes.ControlDark, r1, sf);
				r1.Offset(btnSize + 1, -1);
			}
		}

		private void control_Move(object sender, EventArgs e)
		{
			if (ReferenceEquals(Designer.SelectionService.PrimarySelection, Designer.Control))
			{
				SetNavBoxes();
				Designer.Adorner.Invalidate();
			}
		}

		private void selSvc_SelectionChanged(object sender, EventArgs e)
		{
			if (ReferenceEquals(Designer.SelectionService.PrimarySelection, Designer.Control))
			{
				SetNavBoxes();
				Designer.Adorner.Enabled = true;
				Designer.Control.Owner.DesignerSelected = true;
			}
			else if (Designer.Control.Owner.DesignerSelected)
			{
				Designer.Adorner.Enabled = false;
				Designer.Control.Owner.DesignerSelected = false;
			}
		}

		private void SetNavBoxes()
		{
			Point pt = Designer.BehaviorService.ControlToAdornerWindow(Designer.Control);
			navBox = new Rectangle(pt.X + Designer.Control.Width - navBoxWidth - 17, pt.Y - navBoxHeight - 5, navBoxWidth, navBoxHeight);
		}
	}
}