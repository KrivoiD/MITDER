﻿namespace UsbRelayNet.HidLib {
    /// <summary>
    /// Collected information about connected USB HID.
    /// </summary>
    public class HidDeviceInfo {
        private HidDeviceInfo() {
        }

        internal HidDeviceInfo(string path, int vendorId, int productId, string version, string vendor, string product) {
            this.Path = path;
            this.VendorID = vendorId;
            this.ProductId = productId;
            this.Version = version;
            this.Vendor = vendor;
            this.Product = product;
        }

        /// <summary>
        /// A NULL-terminated string that contains the device interface path. This path can be passed to Win32 functions such as CreateFile.
        /// </summary>
		public string Path { get; private set; }
        /// <summary>
        /// Specifies a HID device's vendor ID.
        /// </summary>
		public int VendorID { get; private set; }
        /// <summary>
        /// Specifies a HID device's product ID.
        /// </summary>
		public int ProductId { get; private set; }
        /// <summary>
        /// Specifies the manufacturer's revision number for a HIDClass device.
        /// </summary>
		public string Version { get; private set; }
        /// <summary>
        /// A top-level collection's embedded string that identifies the manufacturer.
        /// </summary>
		public string Vendor { get; private set; }
        /// <summary>
        /// A top-level collection that identifies the manufacturer's product.
        /// </summary>
		public string Product { get; private set; }
    }
}
