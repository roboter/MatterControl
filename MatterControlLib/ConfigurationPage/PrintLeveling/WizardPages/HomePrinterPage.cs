﻿/*
Copyright (c) 2019, Lars Brubaker, John Lewin
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/

using System;
using MatterHackers.Agg.UI;
using MatterHackers.MatterControl.PrinterCommunication;
using MatterHackers.MatterControl.SlicerConfiguration;

namespace MatterHackers.MatterControl.ConfigurationPage.PrintLeveling
{
	public class HomePrinterPage : WizardPage
	{
		public HomePrinterPage(ISetupWizard setupWizard, string headerText, string instructionsText)
			: base(setupWizard, headerText, instructionsText)
		{
		}

		public override void OnClosed(EventArgs e)
		{
			// Unregister listeners
			printer.Connection.DetailedPrintingStateChanged -= Connection_DetailedPrintingStateChanged;

			base.OnClosed(e);
		}

		public override void OnLoad(EventArgs args)
		{
			printer.Connection.DetailedPrintingStateChanged += Connection_DetailedPrintingStateChanged;

			printer.Connection.HomeAxis(PrinterConnection.Axis.XYZ);

			if(!printer.Settings.GetValue<bool>(SettingsKey.z_homes_to_max))
			{
				// move so we don't heat the printer while the nozzle is touching the bed
				printer.Connection.MoveAbsolute(PrinterConnection.Axis.Z, 10, printer.Settings.Helpers.ManualMovementSpeeds().Z);
			}

			NextButton.Enabled = false;

			base.OnLoad(args);
		}

		private void Connection_DetailedPrintingStateChanged(object sender, EventArgs e)
		{
			if (printer.Connection.DetailedPrintingState != DetailedPrintingState.HomingAxis)
			{
				NextButton.Enabled = true;

				UiThread.RunOnIdle(() => NextButton.InvokeClick());
			}
		}
	}
}