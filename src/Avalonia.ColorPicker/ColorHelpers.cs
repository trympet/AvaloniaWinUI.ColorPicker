using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Avalonia.ColorPicker
{
    internal struct Rgb
    {
        public double R;
        public double G;
        public double B;

        public Rgb(double r, double g, double b)
        {
            R = r;
            G = g;
            B = b;
        }
    }

    internal struct Hsv
    {
        public double H;
        public double S;
        public double V;

        public Hsv(double h, double s, double v)
        {
            H = h;
            S = s;
            V = v;
        }

        public static explicit operator Hsv(Vector4 input)
        {
            return new Hsv(input.X, input.Y, input.Z);
        }

        public static explicit operator Vector4(Hsv input)
        {
            return new Vector4((float)input.H, (float)input.S, (float)input.V, 0);
        }
    }

    internal enum IncrementDirection
    {
        Lower,
        Higher,
    }

    internal enum IncrementAmount
    {
        Small,
        Large,
    }

    public enum ColorPickerHsvChannel
    {
        Hue = 0,
        Saturation = 1,
        Value = 2,
        Alpha = 3,
    }

    internal static class ColorHelpers
    {
        private const int CheckerSize = 4;

        internal static string ToDisplayName(Color color)
        {
            return color.ToString();
        }

        public static Hsv IncrementColorChannel(
            Hsv originalHsv,
            ColorPickerHsvChannel channel,
            IncrementDirection direction,
            IncrementAmount amount,
            bool shouldWrap,
            double minBound,
            double maxBound)
        {
            var newHsv = originalHsv;

            if (amount == IncrementAmount.Small)
            {
                // In order to avoid working with small values that can incur rounding issues,
                // we'll multiple saturation and value by 100 to put them in the range of 0-100 instead of 0-1.
                newHsv.S *= 100;
                newHsv.V *= 100;

                ref var valueToIncrement = ref newHsv.H;
                double incrementAmount = 0;

                // If we're adding a small increment, then we'll just add or subtract 1.
                // If we're adding a large increment, then we want to snap to the next
                // or previous major value - for hue, this is every increment of 30;
                // for saturation and value, this is every increment of 10.
                switch (channel)
                {
                    case ColorPickerHsvChannel.Hue:
                        valueToIncrement = ref newHsv.H;
                        incrementAmount = amount == IncrementAmount.Small ? 1 : 30;
                        break;

                    case ColorPickerHsvChannel.Saturation:
                        valueToIncrement = ref newHsv.S;
                        incrementAmount = amount == IncrementAmount.Small ? 1 : 10;
                        break;

                    case ColorPickerHsvChannel.Value:
                        valueToIncrement = ref newHsv.V;
                        incrementAmount = amount == IncrementAmount.Small ? 1 : 10;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                var previousValue = valueToIncrement;

                valueToIncrement += direction == IncrementDirection.Lower ? -incrementAmount : incrementAmount;

                // If the value has reached outside the bounds, we were previous at the boundary, and we should wrap,
                // then we'll place the selection on the other side of the spectrum.
                // Otherwise, we'll place it on the boundary that was exceeded.
                if (valueToIncrement < minBound)
                {
                    valueToIncrement = (shouldWrap && previousValue == minBound) ? maxBound : minBound;
                }

                if (valueToIncrement > maxBound)
                {
                    valueToIncrement = (shouldWrap && previousValue == maxBound) ? minBound : maxBound;
                }

                // We multiplied saturation and value by 100 previously, so now we want to put them back in the 0-1 range.
                newHsv.S /= 100;
                newHsv.V /= 100;
            }
            else
            {
                // While working with named colors, we're going to need to be working in actual HSV units,
                // so we'll divide the min bound and max bound by 100 in the case of saturation or value,
                // since we'll have received units between 0-100 and we need them within 0-1.
                if (channel == ColorPickerHsvChannel.Saturation ||
                    channel == ColorPickerHsvChannel.Value)
                {
                    minBound /= 100;
                    maxBound /= 100;
                }

                newHsv = FindNextNamedColor(originalHsv, channel, direction, shouldWrap, minBound, maxBound);
            }

            return newHsv;
        }

        internal static Hsv RgbToHsv(Rgb rgb)
        {
            var max = rgb.R >= rgb.G ? (rgb.R >= rgb.B ? rgb.R : rgb.B) : (rgb.G >= rgb.B ? rgb.G : rgb.B);
            var min = rgb.R <= rgb.G ? (rgb.R <= rgb.B ? rgb.R : rgb.B) : (rgb.G <= rgb.B ? rgb.G : rgb.B);

            // The value, a number between 0 and 1, is the largest of R, G, and B (divided by 255).
            // Conceptually speaking, it represents how much color is present.
            // If at least one of R, G, B is 255, then there exists as much color as there can be.
            // If RGB = (0, 0, 0), then there exists no color at all - a value of zero corresponds
            // to black (i.e., the absence of any color).
            var value = max;

            // The "chroma" of the color is a value directly proportional to the extent to which
            // the color diverges from greyscale.  If, for example, we have RGB = (255, 255, 0),
            // then the chroma is maximized - this is a pure yellow, no grey of any kind.
            // On the other hand, if we have RGB = (128, 128, 128), then the chroma being zero
            // implies that this color is pure greyscale, with no actual hue to be found.
            var chroma = max - min;

            double hue;
            double saturation;
            // If the chrome is zero, then hue is technically undefined - a greyscale color
            // has no hue.  For the sake of convenience, we'll just set hue to zero, since
            // it will be unused in this circumstance.  Since the color is purely grey,
            // saturation is also equal to zero - you can think of saturation as basically
            // a measure of hue intensity, such that no hue at all corresponds to a
            // nonexistent intensity.
            if (chroma == 0)
            {
                hue = 0.0;
                saturation = 0.0;
            }
            else
            {
                // In this block, hue is properly defined, so we'll extract both hue
                // and saturation information from the RGB color.

                // Hue can be thought of as a cyclical thing, between 0 degrees and 360 degrees.
                // A hue of 0 degrees is red; 120 degrees is green; 240 degrees is blue; and 360 is back to red.
                // Every other hue is somewhere between either red and green, green and blue, and blue and red,
                // so every other hue can be thought of as an angle on this color wheel.
                // These if/else statements determines where on this color wheel our color lies.
                if (rgb.R == max)
                {
                    // If the red channel is the most pronounced channel, then we exist
                    // somewhere between (-60, 60) on the color wheel - i.e., the section around 0 degrees
                    // where red dominates.  We figure out where in that section we are exactly
                    // by considering whether the green or the blue channel is greater - by subtracting green from blue,
                    // then if green is greater, we'll nudge ourselves closer to 60, whereas if blue is greater, then
                    // we'll nudge ourselves closer to -60.  We then divide by chroma (which will actually make the result larger,
                    // since chroma is a value between 0 and 1) to normalize the value to ensure that we get the right hue
                    // even if we're very close to greyscale.
                    hue = 60 * (rgb.G - rgb.B) / chroma;
                }
                else if (rgb.G == max)
                {
                    // We do the exact same for the case where the green channel is the most pronounced channel,
                    // only this time we want to see if we should tilt towards the blue direction or the red direction.
                    // We add 120 to center our value in the green third of the color wheel.
                    hue = 120 + (60 * (rgb.B - rgb.R) / chroma);
                }
                else // rgb.B == max
                {
                    // And we also do the exact same for the case where the blue channel is the most pronounced channel,
                    // only this time we want to see if we should tilt towards the red direction or the green direction.
                    // We add 240 to center our value in the blue third of the color wheel.
                    hue = 240 + (60 * (rgb.R - rgb.G) / chroma);
                }

                // Since we want to work within the range [0, 360), we'll add 360 to any value less than zero -
                // this will bump red values from within -60 to -1 to 300 to 359.  The hue is the same at both values.
                if (hue < 0.0)
                {
                    hue += 360.0;
                }

                // The saturation, our final HSV axis, can be thought of as a value between 0 and 1 indicating how intense our color is.
                // To find it, we divide the chroma - the distance between the minimum and the maximum RGB channels - by the maximum channel (i.e., the value).
                // This effectively normalizes the chroma - if the maximum is 0.5 and the minimum is 0, the saturation will be (0.5 - 0) / 0.5 = 1,
                // meaning that although this color is not as bright as it can be, the dark color is as intense as it possibly could be.
                // If, on the other hand, the maximum is 0.5 and the minimum is 0.25, then the saturation will be (0.5 - 0.25) / 0.5 = 0.5,
                // meaning that this color is partially washed out.
                // A saturation value of 0 corresponds to a greyscale color, one in which the color is *completely* washed out and there is no actual hue.
                saturation = chroma / value;
            }

            return new Hsv(hue, saturation, value);
        }

        private static Hsv FindNextNamedColor(
    Hsv originalHsv,
    ColorPickerHsvChannel channel,
    IncrementDirection direction,
    bool shouldWrap,
    double minBound,
    double maxBound)
        {
            // There's no easy way to directly get the next named color, so what we'll do
            // is just iterate in the direction that we want to find it until we find a color
            // in that direction that has a color name different than our current color name.
            // Once we find a new color name, then we'll iterate across that color name until
            // we find its bounds on the other side, and then select the color that is exactly
            // in the middle of that color's bounds.
            var newHsv = originalHsv;

            var originalColorName = ToDisplayName(ColorFromRgba(HsvToRgb(originalHsv)));
            var newColorName = originalColorName;

            double originalValue = 0;
            double empty = 0;
            ref var newValue = ref empty;
            double incrementAmount = 0;

            switch (channel)
            {
                case ColorPickerHsvChannel.Hue:
                    originalValue = originalHsv.H;
                    newValue = ref newHsv.H;
                    incrementAmount = 1;
                    break;

                case ColorPickerHsvChannel.Saturation:
                    originalValue = originalHsv.S;
                    newValue = ref newHsv.S;
                    incrementAmount = 0.01;
                    break;

                case ColorPickerHsvChannel.Value:
                    originalValue = originalHsv.V;
                    newValue = ref newHsv.V;
                    incrementAmount = 0.01;
                    break;

                default:
                    throw new NotSupportedException();
            }

            var shouldFindMidPoint = true;

            while (newColorName == originalColorName)
            {
                var previousValue = newValue;
                newValue += (direction == IncrementDirection.Lower ? -1 : 1) * incrementAmount;

                var justWrapped = false;

                // If we've hit a boundary, then either we should wrap or we shouldn't.
                // If we should, then we'll perform that wrapping if we were previously up against
                // the boundary that we've now hit.  Otherwise, we'll stop at that boundary.
                if (newValue > maxBound)
                {
                    if (shouldWrap)
                    {
                        newValue = minBound;
                        justWrapped = true;
                    }
                    else
                    {
                        newValue = maxBound;
                        shouldFindMidPoint = false;
                        newColorName = ToDisplayName(ColorFromRgba(HsvToRgb(newHsv)));
                        break;
                    }
                }
                else if (newValue < minBound)
                {
                    if (shouldWrap)
                    {
                        newValue = maxBound;
                        justWrapped = true;
                    }
                    else
                    {
                        newValue = minBound;
                        shouldFindMidPoint = false;
                        newColorName = ToDisplayName(ColorFromRgba(HsvToRgb(newHsv)));
                        break;
                    }
                }

                if (!justWrapped &&
                    previousValue != originalValue &&
                    Math.Sign(newValue - originalValue) != Math.Sign(previousValue - originalValue))
                {
                    // If we've wrapped all the way back to the start and have failed to find a new color name,
                    // then we'll just quit - there isn't a new color name that we're going to find.
                    shouldFindMidPoint = false;
                    break;
                }

                newColorName = ToDisplayName(ColorFromRgba(HsvToRgb(newHsv)));
            }

            if (shouldFindMidPoint)
            {
                var startHsv = newHsv;
                var currentHsv = startHsv;
                double startEndOffset = 0;
                var currentColorName = newColorName;

                double empty1 = 0, empty2 = 0;
                ref var startValue = ref empty1;
                ref var currentValue = ref empty2;
                double wrapIncrement = 0;

                switch (channel)
                {
                    case ColorPickerHsvChannel.Hue:
                        startValue = ref startHsv.H;
                        currentValue = ref currentHsv.H;
                        wrapIncrement = 360.0;
                        break;

                    case ColorPickerHsvChannel.Saturation:
                        startValue = ref startHsv.S;
                        currentValue = ref currentHsv.S;
                        wrapIncrement = 1.0;
                        break;

                    case ColorPickerHsvChannel.Value:
                        startValue = ref startHsv.V;
                        currentValue = ref currentHsv.V;
                        wrapIncrement = 1.0;
                        break;

                    default:
                        throw new NotSupportedException();
                }

                while (newColorName == currentColorName)
                {
                    currentValue += (direction == IncrementDirection.Lower ? -1 : 1) * incrementAmount;

                    // If we've hit a boundary, then either we should wrap or we shouldn't.
                    // If we should, then we'll perform that wrapping if we were previously up against
                    // the boundary that we've now hit.  Otherwise, we'll stop at that boundary.
                    if (currentValue > maxBound)
                    {
                        if (shouldWrap)
                        {
                            currentValue = minBound;
                            startEndOffset = maxBound - minBound;
                        }
                        else
                        {
                            currentValue = maxBound;
                            break;
                        }
                    }
                    else if (currentValue < minBound)
                    {
                        if (shouldWrap)
                        {
                            currentValue = maxBound;
                            startEndOffset = minBound - maxBound;
                        }
                        else
                        {
                            currentValue = minBound;
                            break;
                        }
                    }

                    currentColorName = ToDisplayName(ColorFromRgba(HsvToRgb(currentHsv)));
                }

                newValue = (startValue + currentValue + startEndOffset) / 2;

                // Dividing by 2 may have gotten us halfway through a single step, so we'll
                // remove that half-step if it exists.
                var leftoverValue = Math.Abs(newValue);

                while (leftoverValue > incrementAmount)
                {
                    leftoverValue -= incrementAmount;
                }

                newValue -= leftoverValue;

                while (newValue < minBound)
                {
                    newValue += wrapIncrement;
                }

                while (newValue > maxBound)
                {
                    newValue -= wrapIncrement;
                }
            }

            return newHsv;
        }

        internal static Rgb HsvToRgb(Hsv hsv)
        {
            var hue = hsv.H;
            var saturation = hsv.S;
            var value = hsv.V;

            // We want the hue to be between 0 and 359,
            // so we first ensure that that's the case.
            while (hue >= 360.0)
            {
                hue -= 360.0;
            }

            while (hue < 0.0)
            {
                hue += 360.0;
            }

            // We similarly clamp saturation and value between 0 and 1.
            saturation = saturation < 0.0 ? 0.0 : saturation;
            saturation = saturation > 1.0 ? 1.0 : saturation;

            value = value < 0.0 ? 0.0 : value;
            value = value > 1.0 ? 1.0 : value;

            // The first thing that we need to do is to determine the chroma (see above for its definition).
            // Remember from above that:
            //
            // 1. The chroma is the difference between the maximum and the minimum of the RGB channels,
            // 2. The value is the maximum of the RGB channels, and
            // 3. The saturation comes from dividing the chroma by the maximum of the RGB channels (i.e., the value).
            //
            // From these facts, you can see that we can retrieve the chroma by simply multiplying the saturation and the value,
            // and we can retrieve the minimum of the RGB channels by subtracting the chroma from the value.
            var chroma = saturation * value;
            var min = value - chroma;

            // If the chroma is zero, then we have a greyscale color.  In that case, the maximum and the minimum RGB channels
            // have the same value (and, indeed, all of the RGB channels are the same), so we can just immediately return
            // the minimum value as the value of all the channels.
            if (chroma == 0)
            {
                return new Rgb(min, min, min);
            }

            // If the chroma is not zero, then we need to continue.  The first step is to figure out
            // what section of the color wheel we're located in.  In order to do that, we'll divide the hue by 60.
            // The resulting value means we're in one of the following locations:
            //
            // 0 - Between red and yellow.
            // 1 - Between yellow and green.
            // 2 - Between green and cyan.
            // 3 - Between cyan and blue.
            // 4 - Between blue and purple.
            // 5 - Between purple and red.
            //
            // In each of these sextants, one of the RGB channels is completely present, one is partially present, and one is not present.
            // For example, as we transition between red and yellow, red is completely present, green is becoming increasingly present, and blue is not present.
            // Then, as we transition from yellow and green, green is now completely present, red is becoming decreasingly present, and blue is still not present.
            // As we transition from green to cyan, green is still completely present, blue is becoming increasingly present, and red is no longer present.  And so on.
            // 
            // To convert from hue to RGB value, we first need to figure out which of the three channels is in which configuration
            // in the sextant that we're located in.  Next, we figure out what value the completely-present color should have.
            // We know that chroma = (max - min), and we know that this color is the max color, so to find its value we simply add
            // min to chroma to retrieve max.  Finally, we consider how far we've transitioned from the pure form of that color
            // to the next color (e.G., how far we are from pure red towards yellow), and give a value to the partially present channel
            // equal to the minimum plus the chroma (i.e., the max minus the min), multiplied by the percentage towards the new color.
            // This gets us a value between the maximum and the minimum representing the partially present channel.
            // Finally, the not-present color must be equal to the minimum value, since it is the one least participating in the overall color.
            var sextant = (int)(hue / 60);
            var intermediateColorPercentage = (hue / 60) - sextant;
            var max = chroma + min;

            double r = 0;
            double g = 0;
            double b = 0;

            switch (sextant)
            {
                case 0:
                    r = max;
                    g = min + (chroma * intermediateColorPercentage);
                    b = min;
                    break;
                case 1:
                    r = min + (chroma * (1 - intermediateColorPercentage));
                    g = max;
                    b = min;
                    break;
                case 2:
                    r = min;
                    g = max;
                    b = min + (chroma * intermediateColorPercentage);
                    break;
                case 3:
                    r = min;
                    g = min + (chroma * (1 - intermediateColorPercentage));
                    b = max;
                    break;
                case 4:
                    r = min + (chroma * intermediateColorPercentage);
                    g = min;
                    b = max;
                    break;
                case 5:
                    r = max;
                    g = min;
                    b = min + (chroma * (1 - intermediateColorPercentage));
                    break;
            }

            return new Rgb(r, g, b);
        }

        public static Color ColorFromRgba(Rgb rgb, double opacity = 1)
        {
            return Color.FromArgb(
                (byte)(opacity * 255),
                (byte)(rgb.R * 255),
                (byte)(rgb.G * 255),
                (byte)(rgb.B * 255));
        }

        public static double IncrementAlphaChannel(
            double originalAlpha,
            IncrementDirection direction,
            IncrementAmount amount,
            bool shouldWrap,
            double minBound,
            double maxBound)
        {    // In order to avoid working with small values that can incur rounding issues,
             // we'll multiple alpha by 100 to put it in the range of 0-100 instead of 0-1.
            originalAlpha *= 100;

            double smallIncrementAmount = 1;
            double largeIncrementAmount = 10;

            if (amount == IncrementAmount.Small)
            {
                originalAlpha += (direction == IncrementDirection.Lower ? -1 : 1) * smallIncrementAmount;
            }
            else
            {
                if (direction == IncrementDirection.Lower)
                {
                    originalAlpha = Math.Ceiling((originalAlpha - largeIncrementAmount) / largeIncrementAmount) * largeIncrementAmount;
                }
                else
                {
                    originalAlpha = Math.Floor((originalAlpha + largeIncrementAmount) / largeIncrementAmount) * largeIncrementAmount;
                }
            }

            // If the value has reached outside the bounds and we should wrap, then we'll place the selection
            // on the other side of the spectrum.  Otherwise, we'll place it on the boundary that was exceeded.
            if (originalAlpha < minBound)
            {
                originalAlpha = shouldWrap ? maxBound : minBound;
            }

            if (originalAlpha > maxBound)
            {
                originalAlpha = shouldWrap ? minBound : maxBound;
            }

            // We multiplied alpha by 100 previously, so now we want to put it back in the 0-1 range.
            return originalAlpha / 100;
        }

        internal static IBitmap CreateBitmapFromPixelData(int pixelWidth, int pixelHeight, List<byte> bgraMinPixelData)
        {
            var bitmap = new WriteableBitmap(new PixelSize(pixelWidth, pixelHeight), new Vector(96, 96), Platform.PixelFormat.Bgra8888, AlphaFormat.Premul);

            using (var fb = bitmap.Lock())
            {
                Marshal.Copy(bgraMinPixelData.ToArray(), 0, fb.Address, bgraMinPixelData.Count);
            }

            return bitmap;
        }

        internal static async Task<IBitmap?> CreateCheckeredBackgroundAsync(
            int width,
            int height,
            Color checkerColor,
            List<byte> bgraCheckeredPixelData,
            CancellationToken cancellationToken)
        {
            if (width == 0 || height == 0)
            {
                return null;
            }

            bgraCheckeredPixelData.Capacity = (int)(width * height * 4);

            var tcs = new TaskCompletionSource<WriteableBitmap?>();

            void WorkItemHandler(CancellationToken cancellationToken)
            {
                for (var y = 0; y < height; y++)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    for (var x = 0; x < width; x++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        // We want the checkered pattern to alternate both vertically and horizontally.
                        // In order to achieve that, we'll toggle visibility of the current pixel on or off
                        // depending on both its x- and its y-position.  If x == CheckerSize, we'll turn visibility off,
                        // but then if y == CheckerSize, we'll turn it back on.
                        // The below is a shorthand for the above intent.
                        var pixelShouldBeBlank = ((x / CheckerSize) + (y / CheckerSize)) % 2 == 0;

                        if (pixelShouldBeBlank)
                        {
                            bgraCheckeredPixelData.Add(0);
                            bgraCheckeredPixelData.Add(0);
                            bgraCheckeredPixelData.Add(0);
                            bgraCheckeredPixelData.Add(0);
                        }
                        else
                        {
                            bgraCheckeredPixelData.Add((byte)(checkerColor.B * checkerColor.A / 255));
                            bgraCheckeredPixelData.Add((byte)(checkerColor.G * checkerColor.A / 255));
                            bgraCheckeredPixelData.Add((byte)(checkerColor.R * checkerColor.A / 255));
                            bgraCheckeredPixelData.Add(checkerColor.A);
                        }
                    }
                }
            }

            await Task.Run(() => WorkItemHandler(cancellationToken), cancellationToken).ConfigureAwait(true);

            if (cancellationToken.IsCancellationRequested)
            {
                return null;
            }

            return CreateBitmapFromPixelData(width, height, bgraCheckeredPixelData);
        }
    }
}
