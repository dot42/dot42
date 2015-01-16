using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Dot42.Ide.WizardForms
{
    internal static class Extensions
    {
        /// <summary>
        /// Show a dialog to accept to all license agreements in the given library nodes.
        /// </summary>
        /// <returns>True if all are accepted, false if at least one is not accepted.</returns>
        internal static bool AcceptToAll(this IEnumerable<LibraryNode> additionalLibs)
        {
            var libsWithLicenses = additionalLibs.Where(x => x.License != null).ToList();
            if (libsWithLicenses.Count == 0)
                return true;

            var licenses = libsWithLicenses.Select(x => x.License).Distinct().ToList();
            // Accept to license agreement
            foreach (var iterator in licenses)
            {
                var license = iterator;
                var libNames = libsWithLicenses.Where(x => x.License == license).Select(x => x.Text);
                using (var dialog = new LicenseAgreementAcceptanceForm(license, libNames))
                {
                    if (dialog.ShowDialog() != DialogResult.OK)
                    {
                        // Not accepted
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
