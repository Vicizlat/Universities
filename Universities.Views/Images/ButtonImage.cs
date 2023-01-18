﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Universities.Views.Images
{
    public class ButtonImage : Image
    {
        public string FileName { get; set; }

        public ButtonImage()
        {
            MouseEnter += Image_MouseEnter;
            MouseLeave += Image_MouseLeave;
            Margin = new Thickness(20, 10, 0, 20);
            Cursor = Cursors.Hand;
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            Source = ImageSource(@$"Images\{FileName}C.png");
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            Source = ImageSource(@$"Images\{FileName}BW.png");
        }

        private static BitmapImage ImageSource(string uriString)
        {
            Uri imageUri = new Uri(uriString, UriKind.Relative);
            return new BitmapImage(imageUri);
        }
    }
}