namespace Dot42.CryptoUI.Controls
{
    internal interface ICertificateWizardPage
    {
        /// <summary>
        /// Should the back button be disabled?
        /// </summary>
        bool IsBackButtonDisabled { get; }

        /// <summary>
        /// Should the next button be disabled?
        /// </summary>
        bool IsNextButtonDisabled { get; }
    }
}
