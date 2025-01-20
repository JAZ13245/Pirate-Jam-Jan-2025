#pragma warning disable 0219
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

#if !DLIB_DONT_USE_UNSAFE_CODE
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
#endif

namespace DlibFaceLandmarkDetector
{
    /// <summary>
    /// Face landmark detector.
    /// </summary>
    public class FaceLandmarkDetector : DisposableDlibObject
    {

        private double[] detectResultBuffer;

        private const int DETECTRESULT_BUFFERSIZE = 6 * 10;

        private double[] detectLandmarkResultBuffer;

        private const int DETECTLANDMARKRESULT_BUFFERSIZE = 2 * 68;

        private void AllocDetectBuffer()
        {
            detectResultBuffer = new double[DETECTRESULT_BUFFERSIZE];

            detectLandmarkResultBuffer = new double[DETECTLANDMARKRESULT_BUFFERSIZE];
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                }

                if (IsEnabledDispose)
                {
                    if (nativeObj != IntPtr.Zero)
                        DlibFaceLandmarkDetector_Dispose(nativeObj);
                    nativeObj = IntPtr.Zero;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Initializes a new instance of the FaceLandmarkDetector class.
        /// </summary>
        /// <remarks>
        /// This instance uses the default frontal face detector.
        /// <br />
        /// ObjectDetector is initialized as follows:
        /// <code>
        /// frontal_face_detector face_detector;
        /// face_detector = get_frontal_face_detector();
        /// </code>
        /// ShapePredictor is initialized as follows:
        /// <code>
        /// shape_predictor sp;
        /// deserialize(shape_predictor_filename) >> sp;
        /// </code>
        /// </remarks>
        /// <param name="shapePredictorFilePath">The file path of the shape predictor.</param>
        public FaceLandmarkDetector(string shapePredictorFilePath)
        {
            nativeObj = DlibFaceLandmarkDetector_Init();

            if (!DlibFaceLandmarkDetector_LoadObjectDetector(nativeObj, null))
            {
                //Debug.LogError ("Failed to load " + objectDetectorFilename);
            }

            if (!DlibFaceLandmarkDetector_LoadShapePredictor(nativeObj, shapePredictorFilePath))
            {
                Debug.LogError("Failed to load " + shapePredictorFilePath);
            }

            AllocDetectBuffer();
        }

        /// <summary>
        /// Initializes a new instance of the FaceLandmarkDetector class.
        /// </summary>
        /// <remarks>
        /// ObjectDetector is initialized with the following code:
        /// <code>
        /// if (object_detector_filename != null)
        /// {
        ///     object_detector<scan_fhog_pyramid<pyramid_down<6>>> simple_detector;
        ///     deserialize(object_detector_filename) >> simple_detector;
        /// }
        /// else
        /// {
        ///     frontal_face_detector face_detector;
        ///     face_detector = get_frontal_face_detector();
        /// }
        /// </code>
        /// ShapePredictor is initialized with the following code:
        /// <code>
        /// shape_predictor sp;
        /// deserialize(shape_predictor_filename) >> sp;
        /// </code>
        /// </remarks>
        /// <param name="objectDetectorFilePath">The file path of the object detector.</param>
        /// <param name="shapePredictorFilePath">The file path of the shape predictor.</param>
        public FaceLandmarkDetector(string objectDetectorFilePath, string shapePredictorFilePath)
        {
            nativeObj = DlibFaceLandmarkDetector_Init();

            if (!DlibFaceLandmarkDetector_LoadObjectDetector(nativeObj, objectDetectorFilePath))
            {
                Debug.LogError("Failed to load " + objectDetectorFilePath);
            }

            if (!DlibFaceLandmarkDetector_LoadShapePredictor(nativeObj, shapePredictorFilePath))
            {
                Debug.LogError("Failed to load " + shapePredictorFilePath);
            }

            AllocDetectBuffer();
        }

        /// <summary>
        /// Sets the image from a Texture2D.
        /// </summary>
        /// <param name="texture2D">
        /// Processing speed is fastest when the TextureFormat is RGBA32, RGB24, or Alpha8.
        /// </param>
        public void SetImage(Texture2D texture2D)
        {
            if (texture2D == null)
                throw new ArgumentNullException("texture2D");
            ThrowIfDisposed();

            TextureFormat format = texture2D.format;
            if (format == TextureFormat.RGBA32 || format == TextureFormat.RGB24 || format == TextureFormat.Alpha8)
            {
#if !DLIB_DONT_USE_UNSAFE_CODE
                unsafe
                {
                    SetImage((IntPtr)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(texture2D.GetRawTextureData<byte>()), texture2D.width, texture2D.height, (int)texture2D.format, true);
                }
#else
                SetImage<byte>(texture2D.GetRawTextureData(), texture2D.width, texture2D.height, (int)texture2D.format, true);
#endif
                return;
            }

            unsafe
            {
                fixed (Color32* ptr = texture2D.GetPixels32())
                {
                    DlibFaceLandmarkDetector_SetImage(nativeObj, (IntPtr)ptr, texture2D.width, texture2D.height, 4, true);
                }
            }
        }

#if !DLIB_DONT_USE_WEBCAMTEXTURE_API

        /// <summary>
        /// Sets the image from a WebCamTexture.
        /// </summary>
        /// <param name="webCamTexture">The WebCamTexture to set as the image.</param>
        public void SetImage(WebCamTexture webCamTexture)
        {
            SetImage(webCamTexture, null);
        }

        /// <summary>
        /// Sets the image from a WebCamTexture.
        /// </summary>
        /// <param name="webCamTexture">The WebCamTexture to set as the image.</param>
        /// <param name="pixels32Buffer">
        /// The optional array to receive pixel data. You can pass in an array of Color32 values to avoid 
        /// allocating new memory each frame. The array should have a length matching the width * height of the texture.
        /// <a href="http://docs.unity3d.com/ScriptReference/WebCamTexture.GetPixels32.html">WebCamTexture.GetPixels32</a>
        /// </param>
        public void SetImage(WebCamTexture webCamTexture, Color32[] pixels32Buffer)
        {
            if (webCamTexture == null)
                throw new ArgumentNullException("webCamTexture");
            ThrowIfDisposed();

            if (pixels32Buffer == null)
            {
                unsafe
                {
                    fixed (Color32* ptr = webCamTexture.GetPixels32())
                    {
                        DlibFaceLandmarkDetector_SetImage(nativeObj, (IntPtr)ptr, webCamTexture.width, webCamTexture.height, 4, true);
                    }
                }
            }
            else
            {
                webCamTexture.GetPixels32(pixels32Buffer);
                unsafe
                {
                    fixed (Color32* ptr = pixels32Buffer)
                    {
                        DlibFaceLandmarkDetector_SetImage(nativeObj, (IntPtr)ptr, webCamTexture.width, webCamTexture.height, 4, true);
                    }
                }
            }
        }

#endif

        /// <summary>
        /// Sets the image from an IntPtr.
        /// </summary>
        /// <param name="intPtr">The IntPtr pointing to the image data.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="bytesPerPixel">The number of bytes per pixel (1, 3, or 4).</param>
        public void SetImage(IntPtr intPtr, int width, int height, int bytesPerPixel)
        {
            SetImage(intPtr, width, height, bytesPerPixel, false);
        }

        /// <summary>
        /// Sets the image from an IntPtr.
        /// </summary>
        /// <param name="intPtr">The IntPtr pointing to the image data.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="bytesPerPixel">The number of bytes per pixel (1, 3, or 4).</param>
        /// <param name="flip">If true, the image will be flipped vertically.</param>
        public void SetImage(IntPtr intPtr, int width, int height, int bytesPerPixel, bool flip)
        {
            if (intPtr == IntPtr.Zero)
                throw new ArgumentException("intPtr == IntPtr.Zero");
            ThrowIfDisposed();

            DlibFaceLandmarkDetector_SetImage(nativeObj, intPtr, width, height, bytesPerPixel, flip);
        }

        /// <summary>
        /// Sets the image from a pixel data array.
        /// </summary>
        /// <param name="array">The array containing pixel data.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="bytesPerPixel">The number of bytes per pixel (1, 3, or 4).</param>
        public void SetImage<T>(T[] array, int width, int height, int bytesPerPixel) where T : unmanaged
        {
            SetImage<T>(array, width, height, bytesPerPixel, false);
        }

        /// <summary>
        /// Sets the image from a pixel data array.
        /// </summary>
        /// <param name="array">The array containing pixel data.</param>
        /// <param name="width">The width of the image.</param>
        /// <param name="height">The height of the image.</param>
        /// <param name="bytesPerPixel">The number of bytes per pixel (1, 3, or 4).</param>
        /// <param name="flip">If true, the image will be flipped vertically.</param>
        public void SetImage<T>(T[] array, int width, int height, int bytesPerPixel, bool flip) where T : unmanaged
        {
            if (array == null)
                throw new ArgumentNullException("array");
            ThrowIfDisposed();

            unsafe
            {
                fixed (T* ptr = array)
                {
                    DlibFaceLandmarkDetector_SetImage(nativeObj, (IntPtr)ptr, width, height, bytesPerPixel, flip);
                }
            }
        }

        /// <summary>
        /// Represents the detection result of an object, including its bounding rectangle, detection confidence, and weight index.
        /// </summary>
        public class RectDetection
        {
            /// <summary>
            /// The bounding rectangle of the detected object.
            /// </summary>
            public Rect rect;

            /// <summary>
            /// The confidence level of the detection (between 0.0 and 1.0).
            /// A higher value indicates higher confidence in the detection.
            /// </summary>
            public double detection_confidence;

            /// <summary>
            /// The index for the weight vector that generated this detection.
            /// </summary>
            public long weight_index;

            /// <summary>
            /// Initializes a new instance of the <see cref="RectDetection"/> class with default values.
            /// </summary>
            public RectDetection()
            {
                rect = new Rect();
                detection_confidence = 0.0;
                weight_index = 0;
            }
        }

        /// <summary>
        /// Detects objects in the image.
        /// </summary>
        /// <returns>A list of <see cref="Rect"/> representing the detected objects.</returns>
        public List<Rect> Detect()
        {
            return Detect(0.0);
        }

        /// <summary>
        /// Detects objects in the image.
        /// </summary>
        /// <param name="adjust_threshold">A parameter to adjust the detection threshold.</param>
        /// <returns>A list of <see cref="Rect"/> representing the detected objects.</returns>
        public List<Rect> Detect(double adjust_threshold)
        {
            ThrowIfDisposed();

            List<Rect> rects = new List<Rect>();

            int detectCount = DlibFaceLandmarkDetector_Detect(nativeObj, adjust_threshold);
            if (detectCount > 0)
            {
                if (detectResultBuffer.Length < 6 * detectCount) detectResultBuffer = new double[6 * detectCount];

                DlibFaceLandmarkDetector_GetDetectResult(nativeObj, detectResultBuffer);

                for (int i = 0; i < detectCount; i++)
                {
                    rects.Add(new Rect((float)detectResultBuffer[i * 6 + 0], (float)detectResultBuffer[i * 6 + 1], (float)detectResultBuffer[i * 6 + 2], (float)detectResultBuffer[i * 6 + 3]));
                }
            }

            return rects;
        }

        /// <summary>
        /// Detects objects in the image.
        /// </summary>
        /// <returns>A list of <see cref="RectDetection"/> representing the detected objects.</returns>
        public List<RectDetection> DetectRectDetection()
        {
            return DetectRectDetection(0.0);
        }

        /// <summary>
        /// Detects objects in the image.
        /// </summary>
        /// <param name="adjust_threshold">A parameter to adjust the detection threshold.</param>
        /// <returns>A list of <see cref="RectDetection"/> representing the detected objects.</returns>
        public List<RectDetection> DetectRectDetection(double adjust_threshold)
        {
            ThrowIfDisposed();

            List<RectDetection> rectDetections = new List<RectDetection>();

            int detectCount = DlibFaceLandmarkDetector_Detect(nativeObj, adjust_threshold);
            if (detectCount > 0)
            {
                if (detectResultBuffer.Length < 6 * detectCount) detectResultBuffer = new double[6 * detectCount];

                DlibFaceLandmarkDetector_GetDetectResult(nativeObj, detectResultBuffer);

                for (int i = 0; i < detectCount; i++)
                {
                    RectDetection rectDetection = new RectDetection();
                    rectDetection.rect = new Rect((float)detectResultBuffer[i * 6 + 0], (float)detectResultBuffer[i * 6 + 1], (float)detectResultBuffer[i * 6 + 2], (float)detectResultBuffer[i * 6 + 3]);
                    rectDetection.detection_confidence = detectResultBuffer[i * 6 + 4];
                    rectDetection.weight_index = (long)detectResultBuffer[i * 6 + 5];

                    rectDetections.Add(rectDetection);
                }
            }

            return rectDetections;
        }

        /// <summary>
        /// Detects objects in the image.
        /// </summary>
        /// <returns>
        /// An array of doubles representing detected object data. The array contains values in the following order:
        /// <list type="bullet">
        /// <item>left_0, top_0, width_0, height_0, detection_confidence_0, weight_index_0</item>
        /// <item>left_1, top_1, width_1, height_1, detection_confidence_1, weight_index_1</item>
        /// <item>...</item>
        /// </list>
        /// </returns>
        public double[] DetectArray()
        {
            return DetectArray(0.0);
        }

        /// <summary>
        /// Detects objects in the image.
        /// </summary>
        /// <param name="adjust_threshold">A parameter to adjust the detection threshold.</param>
        /// <returns>
        /// An array of doubles representing detected object data. The array contains values in the following order:
        /// <list type="bullet">
        /// <item>left_0, top_0, width_0, height_0, detection_confidence_0, weight_index_0</item>
        /// <item>left_1, top_1, width_1, height_1, detection_confidence_1, weight_index_1</item>
        /// <item>...</item>
        /// </list>
        /// </returns>
        public double[] DetectArray(double adjust_threshold)
        {
            ThrowIfDisposed();

            int detectCount = DlibFaceLandmarkDetector_Detect(nativeObj, adjust_threshold);
            if (detectCount > 0)
            {

                if (detectResultBuffer.Length < 6 * detectCount) detectResultBuffer = new double[6 * detectCount];

                DlibFaceLandmarkDetector_GetDetectResult(nativeObj, detectResultBuffer);


#if NET_STANDARD_2_1
                double[] result = (new ArraySegment<double>(detectResultBuffer, 0, 6 * detectCount)).ToArray();
#else
                double[] result = new double[6 * detectCount];
                Array.Copy(detectResultBuffer, result, result.Length);
#endif

                return result;
            }

            return new double[0];
        }

        /// <summary>
        /// Detects objects in the image.
        /// </summary>
        /// <returns>
        /// An array of <see cref="ValueTuple{Double, Double, Double, Double}"/> representing detected object data. Each ValueTuple contains values in the following order:
        /// <list type="bullet">
        /// <item>x_0 (left), y_0 (top), width_0, height_0</item>
        /// <item>x_1 (left), y_1 (top), width_1, height_1</item>
        /// <item>...</item>
        /// </list>
        /// </returns>
        public (double x, double y, double width, double height)[] DetectValueTuple()
        {
            return DetectValueTuple(0.0);
        }

        /// <summary>
        /// Detects objects in the image.
        /// </summary>
        /// <param name="adjust_threshold">A parameter to adjust the detection threshold.</param>
        /// <returns>
        /// An array of <see cref="ValueTuple{Double, Double, Double, Double}"/> representing detected object data. Each ValueTuple contains values in the following order:
        /// <list type="bullet">
        /// <item>x_0 (left), y_0 (top), width_0, height_0</item>
        /// <item>x_1 (left), y_1 (top), width_1, height_1</item>
        /// <item>...</item>
        /// </list>
        /// </returns>
        public (double x, double y, double width, double height)[] DetectValueTuple(double adjust_threshold)
        {
            ThrowIfDisposed();

            int detectCount = DlibFaceLandmarkDetector_Detect(nativeObj, adjust_threshold);
            if (detectCount > 0)
            {

                if (detectResultBuffer.Length < 6 * detectCount) detectResultBuffer = new double[6 * detectCount];

                DlibFaceLandmarkDetector_GetDetectResult(nativeObj, detectResultBuffer);

                (double x, double y, double width, double height)[] result = new (double x, double y, double width, double height)[detectCount];
                for (int i = 0; i < result.Length; i++)
                {
                    result[i].x = detectResultBuffer[i * 6 + 0];
                    result[i].y = detectResultBuffer[i * 6 + 1];
                    result[i].width = detectResultBuffer[i * 6 + 2];
                    result[i].height = detectResultBuffer[i * 6 + 3];
                }

                return result;
            }

            return default;
        }

        /// <summary>
        /// Detects objects and returns the number of objects detected.
        /// </summary>
        /// <returns>
        /// The number of objects detected.
        /// </returns>
        public int DetectOnly()
        {

            return DetectOnly(0.0);

        }

        /// <summary>
        /// Detects objects and returns the number of objects detected.
        /// </summary>
        /// <param name="adjust_threshold">A parameter to adjust the detection threshold.</param>
        /// <returns>
        /// The number of objects detected.
        /// </returns>
        public int DetectOnly(double adjust_threshold)
        {
            ThrowIfDisposed();

            return DlibFaceLandmarkDetector_Detect(nativeObj, adjust_threshold);

        }

        /// <summary>
        /// Gets the result data of the objects detected by the DetectOnly() method, 
        /// passing a data size of DetectOnly() * 6 as an argument. This method can 
        /// retrieve results without memory allocation.
        /// </summary>
        /// <param name="result">
        /// An array of doubles representing detected object data. The array contains values in the following order:
        /// <list type="bullet">
        /// <item>left_0, top_0, width_0, height_0, detection_confidence_0, weight_index_0</item>
        /// <item>left_1, top_1, width_1, height_1, detection_confidence_1, weight_index_1</item>
        /// <item>...</item>
        /// </list>
        /// The array is passed by reference to receive the detection result data without allocating new memory each time.
        /// </param>
        public void GetDetectResult(double[] result)
        {
            ThrowIfDisposed();

            unsafe
            {
                fixed (double* ptr = result)
                {
                    DlibFaceLandmarkDetector_GetDetectResult(nativeObj, (IntPtr)ptr);
                }
            }

        }

#if NET_STANDARD_2_1

        /// <summary>
        /// Gets the result data of the objects detected by the DetectOnly() method, 
        /// passing a data size of DetectOnly() * 6 as an argument. This method can 
        /// retrieve results without memory allocation.
        /// </summary>
        /// <param name="result">
        /// A span of doubles representing detected object data. The span contains values in the following order:
        /// <list type="bullet">
        /// <item>left_0, top_0, width_0, height_0, detection_confidence_0, weight_index_0</item>
        /// <item>left_1, top_1, width_1, height_1, detection_confidence_1, weight_index_1</item>
        /// <item>...</item>
        /// </list>
        /// The span is passed by reference to receive the detection result data without allocating new memory each time.
        /// </param>
        public void GetDetectResult(Span<double> result)
        {
            ThrowIfDisposed();

            unsafe
            {
                fixed (double* ptr = result)
                {
                    DlibFaceLandmarkDetector_GetDetectResult(nativeObj, (IntPtr)ptr);
                }
            }

        }

#endif

        /// <summary>
        /// Detects the object landmarks within the specified region.
        /// </summary>
        /// <param name="left">The left coordinate of the region to search for landmarks.</param>
        /// <param name="top">The top coordinate of the region to search for landmarks.</param>
        /// <param name="width">The width of the region to search for landmarks.</param>
        /// <param name="height">The height of the region to search for landmarks.</param>
        /// <returns>
        /// A list of <see cref="Vector2"/> representing the detected object landmarks.
        /// </returns>
        public List<Vector2> DetectLandmark(double left, double top, double width, double height)
        {
            ThrowIfDisposed();

            List<Vector2> points = new List<Vector2>();

            int detectCount = DlibFaceLandmarkDetector_DetectLandmark(nativeObj, left, top, width, height);
            if (detectCount > 0)
            {
                if (detectLandmarkResultBuffer.Length < 2 * detectCount) detectLandmarkResultBuffer = new double[2 * detectCount];

                DlibFaceLandmarkDetector_GetDetectLandmarkResult(nativeObj, detectLandmarkResultBuffer);

                for (int i = 0; i < detectCount; i++)
                {
                    points.Add(new Vector2((float)detectLandmarkResultBuffer[i * 2 + 0], (float)detectLandmarkResultBuffer[i * 2 + 1]));
                }
            }

            return points;
        }

        /// <summary>
        /// Detects the object landmarks within the specified region defined by a rectangle.
        /// </summary>
        /// <param name="rect">A rectangle defining the region to search for landmarks. The rectangle is defined by its left, top, width, and height.</param>
        /// <returns>
        /// A list of <see cref="Vector2"/> representing the detected object landmarks.
        /// </returns>
        public List<Vector2> DetectLandmark(Rect rect)
        {
            return DetectLandmark(rect.xMin, rect.yMin, rect.width, rect.height);
        }

        /// <summary>
        /// Detects the object landmarks within the specified region defined by a rectangle.
        /// </summary>
        /// <param name="rect">
        /// A <see cref="ValueTuple{Double, Double, Double, Double}"/> representing a rectangle. The ValueTuple contains the following values:
        /// <list type="bullet">
        /// <item>x (left), y (top), width, height</item>
        /// </list>
        /// </param>
        /// <returns>
        /// An array of <see cref="ValueTuple{Double, Double}"/> representing the detected object landmarks.
        /// Each ValueTuple contains the x and y coordinates of a detected landmark.
        /// If no landmarks are detected, the method returns the default value (null).
        /// </returns>
        public (double x, double y)[] DetectLandmark(in (double x, double y, double width, double height) rect)
        {
            ThrowIfDisposed();

            int detectCount = DlibFaceLandmarkDetector_DetectLandmark(nativeObj, rect.x, rect.y, rect.width, rect.height);
            if (detectCount > 0)
            {
                if (detectLandmarkResultBuffer.Length < 2 * detectCount) detectLandmarkResultBuffer = new double[2 * detectCount];

                DlibFaceLandmarkDetector_GetDetectLandmarkResult(nativeObj, detectLandmarkResultBuffer);

                (double x, double y)[] points = new (double x, double y)[detectCount];
                for (int i = 0; i < detectCount; i++)
                {
                    points[i].x = detectLandmarkResultBuffer[i * 2 + 0];
                    points[i].y = detectLandmarkResultBuffer[i * 2 + 1];
                }

                return points;
            }

            return default;
        }

        /// <summary>
        /// Detects the object landmarks within the specified region defined by a rectangle.
        /// </summary>
        /// <param name="left">The left coordinate of the rectangle.</param>
        /// <param name="top">The top coordinate of the rectangle.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        /// <returns>
        /// An array of doubles representing the detected object landmark data. The array contains values in the following order:
        /// <list type="bullet">
        /// <item>x_0, y_0</item>
        /// <item>x_1, y_1</item>
        /// <item>...</item>
        /// </list>
        /// </returns>
        public double[] DetectLandmarkArray(double left, double top, double width, double height)
        {
            ThrowIfDisposed();

            int detectCount = DlibFaceLandmarkDetector_DetectLandmark(nativeObj, left, top, width, height);
            if (detectCount > 0)
            {
                if (detectLandmarkResultBuffer.Length < 2 * detectCount) detectLandmarkResultBuffer = new double[2 * detectCount];

                DlibFaceLandmarkDetector_GetDetectLandmarkResult(nativeObj, detectLandmarkResultBuffer);

#if NET_STANDARD_2_1
                double[] result = (new ArraySegment<double>(detectLandmarkResultBuffer, 0, 2 * detectCount)).ToArray();
#else
                double[] result = new double[2 * detectCount];
                Array.Copy(detectLandmarkResultBuffer, result, result.Length);
#endif

                return result;
            }

            return new double[0];
        }

        /// <summary>
        /// Detects the object landmarks within the specified region defined by a rectangle.
        /// </summary>
        /// <param name="rect">
        /// The rectangle defining the region in which to detect object landmarks.
        /// </param>
        /// <returns>
        /// An array of doubles representing the detected object landmark data. The array contains values in the following order:
        /// <list type="bullet">
        /// <item>x_0, y_0</item>
        /// <item>x_1, y_1</item>
        /// <item>...</item>
        /// </list>
        /// </returns>
        public double[] DetectLandmarkArray(Rect rect)
        {
            return DetectLandmarkArray(rect.xMin, rect.yMin, rect.width, rect.height);
        }

        /// <summary>
        /// Detects the object landmarks within the specified region defined by a rectangle.
        /// </summary>
        /// <param name="rect">
        /// A <see cref="ValueTuple{Double, Double, Double, Double}"/> representing a rectangle. The ValueTuple contains the following values:
        /// <list type="bullet">
        /// <item>x (left), y (top), width, height</item>
        /// </list>
        /// </param>
        /// <returns>
        /// An array of doubles representing the detected object landmark data. The array contains values in the following order:
        /// <list type="bullet">
        /// <item>x_0, y_0</item>
        /// <item>x_1, y_1</item>
        /// <item>...</item>
        /// </list>
        /// If no landmarks are detected, the method returns the default value (null).
        /// </returns>
        public double[] DetectLandmarkArray(in (double x, double y, double width, double height) rect)
        {
            return DetectLandmarkArray(rect.x, rect.y, rect.width, rect.height);
        }

        /// <summary>
        /// Detects objects and returns the number of detected object landmarks.
        /// </summary>
        /// <param name="left">
        /// The left coordinate of the object region to detect landmarks.
        /// </param>
        /// <param name="top">
        /// The top coordinate of the object region to detect landmarks.
        /// </param>
        /// <param name="width">
        /// The width of the object region to detect landmarks.
        /// </param>
        /// <param name="height">
        /// The height of the object region to detect landmarks.
        /// </param>
        /// <returns>
        /// An integer representing the number of detected object landmarks.
        /// </returns>
        public int DetectLandmarkOnly(double left, double top, double width, double height)
        {
            ThrowIfDisposed();

            return DlibFaceLandmarkDetector_DetectLandmark(nativeObj, left, top, width, height);

        }

        /// <summary>
        /// Detects objects and returns the number of detected object landmarks within the specified region defined by a rectangle.
        /// </summary>
        /// <param name="rect">
        /// The rectangle defining the region in which to detect object landmarks.
        /// </param>
        /// <returns>
        /// An integer representing the number of detected object landmarks.
        /// </returns>
        public int DetectLandmarkOnly(Rect rect)
        {

            return DetectLandmarkOnly(rect.xMin, rect.yMin, rect.width, rect.height);

        }

        /// <summary>
        /// Detects objects and returns the number of detected object landmarks within the specified region defined by a rectangle.
        /// </summary>
        /// <param name="rect">
        /// A <see cref="ValueTuple{Double, Double, Double, Double}"/> representing a rectangle. The ValueTuple contains the following values:
        /// <list type="bullet">
        /// <item>x (left), y (top), width, height</item>
        /// </list>
        /// </param>
        /// <returns>
        /// An integer representing the number of object landmarks detected within the specified region.
        /// </returns>
        public int DetectLandmarkOnly(in (double x, double y, double width, double height) rect)
        {

            return DetectLandmarkOnly(rect.x, rect.y, rect.width, rect.height);

        }

        /// <summary>
        /// Gets the result data of the object landmarks detected by the DetectLandmarkOnly() method, 
        /// passing a data size of DetectLandmarkOnly() * 2 as an argument. This method can 
        /// retrieve results without memory allocation.
        /// </summary>
        /// <param name="result">
        /// An array of doubles representing detected object landmark data. The array contains values in the following order:
        /// <list type="bullet">
        /// <item>x_0, y_0</item>
        /// <item>x_1, y_1</item>
        /// <item>...</item>
        /// </list>
        /// The array is passed by reference to receive the landmark result data without allocating new memory each time.
        /// </param>
        public void GetDetectLandmarkResult(double[] result)
        {
            ThrowIfDisposed();

            unsafe
            {
                fixed (double* ptr = result)
                {
                    DlibFaceLandmarkDetector_GetDetectLandmarkResult(nativeObj, (IntPtr)ptr);
                }
            }
        }

#if NET_STANDARD_2_1

        /// <summary>
        /// Gets the result data of the object landmarks detected by the DetectLandmarkOnly() method, 
        /// passing a data size of DetectLandmarkOnly() * 2 as an argument. This method can 
        /// retrieve results without memory allocation.
        /// </summary>
        /// <param name="result">
        /// A span of doubles representing detected object landmark data. The span contains values in the following order:
        /// <list type="bullet">
        /// <item>x_0, y_0</item>
        /// <item>x_1, y_1</item>
        /// <item>...</item>
        /// </list>
        /// The span is passed by reference to receive the landmark result data without allocating new memory each time.
        /// </param>
        public void GetDetectLandmarkResult(Span<double> result)
        {
            ThrowIfDisposed();

            unsafe
            {
                fixed (double* ptr = result)
                {
                    DlibFaceLandmarkDetector_GetDetectLandmarkResult(nativeObj, (IntPtr)ptr);
                }
            }
        }

#endif

        /// <summary>
        /// Determines whether all of the object parts are contained within the object rectangle.
        /// </summary>
        /// <returns>
        /// A boolean value indicating whether all of the object parts are inside the rectangle. 
        /// <c>true</c> if all points are contained within the rectangle, otherwise <c>false</c>.
        /// </returns>
        public bool IsAllPartsInRect()
        {
            ThrowIfDisposed();

            bool flag = DlibFaceLandmarkDetector_IsAllPartsInRect(nativeObj);

            return flag;
        }

        /// <summary>
        /// Gets the number of parts in the shape predictor.
        /// </summary>
        /// <returns>
        /// A <see cref="long"/> representing the number of parts in the shape predictor.
        /// </returns>
        public long GetShapePredictorNumParts()
        {
            ThrowIfDisposed();

            long numParts = DlibFaceLandmarkDetector_ShapePredictorNumParts(nativeObj);

            return numParts;
        }

        /// <summary>
        /// Gets the number of features in the shape predictor.
        /// </summary>
        /// <returns>
        /// A <see cref="long"/> representing the number of features in the shape predictor.
        /// </returns>
        public long GetShapePredictorNumFeatures()
        {
            ThrowIfDisposed();

            long numFeatures = DlibFaceLandmarkDetector_ShapePredictorNumFeatures(nativeObj);

            return numFeatures;
        }

        /// <summary>
        /// Draws the detection result on the specified texture.
        /// </summary>
        /// <param name="texture2D">
        /// The <see cref="Texture2D"/> on which the detection result will be drawn. 
        /// Processing speed is fastest when the texture format is <see cref="TextureFormat.RGBA32"/>, <see cref="TextureFormat.RGB24"/>, or <see cref="TextureFormat.Alpha8"/>.
        /// </param>
        /// <param name="r">
        /// The red component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="g">
        /// The green component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="b">
        /// The blue component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="a">
        /// The alpha (transparency) component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="thickness">
        /// The thickness of the lines used to draw the detection results.
        /// </param>
        /// <param name="updateMipmaps">
        /// If set to true, mipmap levels of the texture will be recalculated.
        /// </param>
        /// <param name="makeNoLongerReadable">
        /// If set to true, the system memory copy of the texture will be released, making it no longer readable.
        /// </param>
        public void DrawDetectResult(Texture2D texture2D, int r, int g, int b, int a, int thickness, bool updateMipmaps = false, bool makeNoLongerReadable = false)
        {
            DrawDetectResult(texture2D, r, g, b, a, thickness, null, null, updateMipmaps, makeNoLongerReadable);
        }

        /// <summary>
        /// Draws the detection result on the specified texture.
        /// </summary>
        /// <param name="texture2D">
        /// The <see cref="Texture2D"/> on which the detection result will be drawn. 
        /// Processing speed is fastest when the texture format is <see cref="TextureFormat.RGBA32"/>, <see cref="TextureFormat.RGB24"/>, or <see cref="TextureFormat.Alpha8"/>.
        /// </param>
        /// <param name="r">
        /// The red component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="g">
        /// The green component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="b">
        /// The blue component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="a">
        /// The alpha (transparency) component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="thickness">
        /// The thickness of the lines used to draw the detection results.
        /// </param>
        /// <param name="pixels32Buffer">
        /// An optional array to receive the <see cref="Color32"/> pixel data. You can pass in an array to avoid allocating new memory each frame. 
        /// The array must be initialized to a length matching the width * height of the texture.
        /// <a href="http://docs.unity3d.com/ScriptReference/WebCamTexture.GetPixels32.html">WebCamTexture.GetPixels32</a>.
        /// </param>
        /// <param name="rawTextureDataBuffer">
        /// An optional array to receive raw texture data. You can pass in an array to avoid allocating new memory each frame.
        /// The array must be initialized to a length matching the raw data size of the texture.
        /// <a href="https://docs.unity3d.com/ScriptReference/Texture2D.GetRawTextureData.html">Texture2D.GetRawTextureData</a>.
        /// </param>
        /// <param name="updateMipmaps">
        /// If set to true, mipmap levels of the texture will be recalculated.
        /// </param>
        /// <param name="makeNoLongerReadable">
        /// If set to true, the system memory copy of the texture will be released, making it no longer readable.
        /// </param>
        public void DrawDetectResult(Texture2D texture2D, int r, int g, int b, int a, int thickness, Color32[] pixels32Buffer, byte[] rawTextureDataBuffer = null, bool updateMipmaps = false, bool makeNoLongerReadable = false)
        {
            if (texture2D == null)
                throw new ArgumentNullException("texture2D");
            ThrowIfDisposed();

            TextureFormat format = texture2D.format;
            if (format == TextureFormat.RGBA32 || format == TextureFormat.RGB24 || format == TextureFormat.Alpha8)
            {
#if !DLIB_DONT_USE_UNSAFE_CODE
                unsafe
                {
                    NativeArray<byte> rawTextureData = texture2D.GetRawTextureData<byte>();

                    DlibFaceLandmarkDetector_DrawDetectResult(nativeObj, (IntPtr)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(rawTextureData), texture2D.width, texture2D.height, (int)texture2D.format, true, r, g, b, a, thickness);
                }
                texture2D.Apply(updateMipmaps, makeNoLongerReadable);

                return;
#else
                if ((rawTextureDataBuffer != null) || (pixels32Buffer == null && texture2D.mipmapCount == 1))
                {
                    GCHandle rawTextureDataHandle;
                    if (rawTextureDataBuffer == null)
                    {
                        byte[] rawTextureData = texture2D.GetRawTextureData();
                        rawTextureDataHandle = GCHandle.Alloc(rawTextureData, GCHandleType.Pinned);
                        DlibFaceLandmarkDetector_DrawDetectResult(nativeObj, rawTextureDataHandle.AddrOfPinnedObject(), texture2D.width, texture2D.height, (int)texture2D.format, true, r, g, b, a, thickness);
                        texture2D.LoadRawTextureData(rawTextureDataHandle.AddrOfPinnedObject(), rawTextureData.Length);
                    }
                    else
                    {
                        rawTextureDataHandle = GCHandle.Alloc(rawTextureDataBuffer, GCHandleType.Pinned);
                        DlibFaceLandmarkDetector_DrawDetectResult(nativeObj, rawTextureDataHandle.AddrOfPinnedObject(), texture2D.width, texture2D.height, (int)texture2D.format, true, r, g, b, a, thickness);
                        texture2D.LoadRawTextureData(rawTextureDataHandle.AddrOfPinnedObject(), rawTextureDataBuffer.Length);
                    }
                    texture2D.Apply(updateMipmaps, makeNoLongerReadable);
                    rawTextureDataHandle.Free();

                    return;
                }
#endif
            }

            //You can use SetPixels32 with the following texture formats:
            //https://docs.unity3d.com/2020.3/Documentation/ScriptReference/Texture2D.SetPixels32.html
#if !DLIB_DONT_USE_UNSAFE_CODE
            if (pixels32Buffer == null)
            {
                Color32[] pixels32 = texture2D.GetPixels32();
                unsafe
                {
                    fixed (Color32* ptr = pixels32)
                    {
                        DlibFaceLandmarkDetector_DrawDetectResult(nativeObj, (IntPtr)ptr, texture2D.width, texture2D.height, 4, true, r, g, b, a, thickness);
                    }
                }
                texture2D.SetPixels32(pixels32);
            }
            else
            {
                unsafe
                {
                    fixed (Color32* ptr = pixels32Buffer)
                    {
                        DlibFaceLandmarkDetector_DrawDetectResult(nativeObj, (IntPtr)ptr, texture2D.width, texture2D.height, 4, true, r, g, b, a, thickness);
                    }
                }
                texture2D.SetPixels32(pixels32Buffer);
            }
            texture2D.Apply(updateMipmaps, makeNoLongerReadable);
#else
            GCHandle pixels32Handle;
            if (pixels32Buffer == null)
            {
                Color32[] pixels32 = texture2D.GetPixels32();

                pixels32Handle = GCHandle.Alloc(pixels32, GCHandleType.Pinned);
                DlibFaceLandmarkDetector_DrawDetectResult(nativeObj, pixels32Handle.AddrOfPinnedObject(), texture2D.width, texture2D.height, 4, true, r, g, b, a, thickness);

                texture2D.SetPixels32(pixels32);
            }
            else
            {
                pixels32Handle = GCHandle.Alloc(pixels32Buffer, GCHandleType.Pinned);
                DlibFaceLandmarkDetector_DrawDetectResult(nativeObj, pixels32Handle.AddrOfPinnedObject(), texture2D.width, texture2D.height, 4, true, r, g, b, a, thickness);

                texture2D.SetPixels32(pixels32Buffer);
            }
            texture2D.Apply(updateMipmaps, makeNoLongerReadable);
            pixels32Handle.Free();
#endif
        }

        /// <summary>
        /// Draws the detection result on a memory buffer.
        /// </summary>
        /// <param name="intPtr">
        /// The pointer to the memory buffer where the detection result will be drawn. 
        /// </param>
        /// <param name="width">
        /// The width of the image (in pixels) where the detection result will be drawn.
        /// </param>
        /// <param name="height">
        /// The height of the image (in pixels) where the detection result will be drawn.
        /// </param>
        /// <param name="bytesPerPixel">
        /// The number of bytes per pixel in the memory buffer. Valid values are 1 (grayscale), 3 (RGB), or 4 (RGBA).
        /// </param>
        /// <param name="r">
        /// The red component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="g">
        /// The green component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="b">
        /// The blue component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="a">
        /// The alpha (transparency) component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="thickness">
        /// The thickness of the lines used to draw the detection results.
        /// </param>
        public void DrawDetectResult(IntPtr intPtr, int width, int height, int bytesPerPixel, int r, int g, int b, int a, int thickness)
        {
            DrawDetectResult(intPtr, width, height, bytesPerPixel, false, r, g, b, a, thickness);
        }

        /// <summary>
        /// Draws the detection result on a memory buffer with optional vertical flipping.
        /// </summary>
        /// <param name="intPtr">
        /// The pointer to the memory buffer where the detection result will be drawn. 
        /// </param>
        /// <param name="width">
        /// The width of the image (in pixels) where the detection result will be drawn.
        /// </param>
        /// <param name="height">
        /// The height of the image (in pixels) where the detection result will be drawn.
        /// </param>
        /// <param name="bytesPerPixel">
        /// The number of bytes per pixel in the memory buffer. Valid values are 1 (grayscale), 3 (RGB), or 4 (RGBA).
        /// </param>
        /// <param name="flip">
        /// A boolean flag indicating whether to flip the image vertically. 
        /// Set to <c>true</c> to flip, or <c>false</c> to leave the image unflipped.
        /// </param>
        /// <param name="r">
        /// The red component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="g">
        /// The green component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="b">
        /// The blue component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="a">
        /// The alpha (transparency) component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="thickness">
        /// The thickness of the lines used to draw the detection results.
        /// </param>
        public void DrawDetectResult(IntPtr intPtr, int width, int height, int bytesPerPixel, bool flip, int r, int g, int b, int a, int thickness)
        {
            if (intPtr == IntPtr.Zero)
                throw new ArgumentException("intPtr == IntPtr.Zero");
            ThrowIfDisposed();

            DlibFaceLandmarkDetector_DrawDetectResult(nativeObj, intPtr, width, height, bytesPerPixel, flip, r, g, b, a, thickness);
        }

        /// <summary>
        /// Draws the detection result on an array representing the image data.
        /// </summary>
        /// <param name="array">
        /// The array representing the image data where the detection result will be drawn. 
        /// The array should contain pixel data in the format corresponding to <paramref name="bytesPerPixel"/>.
        /// </param>
        /// <param name="width">
        /// The width of the image (in pixels) where the detection result will be drawn.
        /// </param>
        /// <param name="height">
        /// The height of the image (in pixels) where the detection result will be drawn.
        /// </param>
        /// <param name="bytesPerPixel">
        /// The number of bytes per pixel in the memory buffer. Valid values are 1 (grayscale), 3 (RGB), or 4 (RGBA).
        /// </param>
        /// <param name="r">
        /// The red component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="g">
        /// The green component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="b">
        /// The blue component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="a">
        /// The alpha (transparency) component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="thickness">
        /// The thickness of the lines used to draw the detection results.
        /// </param>
        public void DrawDetectResult<T>(T[] array, int width, int height, int bytesPerPixel, int r, int g, int b, int a, int thickness) where T : unmanaged
        {
            DrawDetectResult<T>(array, width, height, bytesPerPixel, false, r, g, b, a, thickness);
        }

        /// <summary>
        /// Draws the detection result on an array representing the image data.
        /// </summary>
        /// <param name="array">
        /// The array representing the image data where the detection result will be drawn. 
        /// The array should contain pixel data in the format corresponding to <paramref name="bytesPerPixel"/>.
        /// </param>
        /// <param name="width">
        /// The width of the image (in pixels) where the detection result will be drawn.
        /// </param>
        /// <param name="height">
        /// The height of the image (in pixels) where the detection result will be drawn.
        /// </param>
        /// <param name="bytesPerPixel">
        /// The number of bytes per pixel in the memory buffer. Valid values are 1 (grayscale), 3 (RGB), or 4 (RGBA).
        /// </param>
        /// <param name="flip">
        /// A boolean flag indicating whether to flip the image vertically. 
        /// Set to <c>true</c> to flip, or <c>false</c> to leave the image unflipped.
        /// </param>
        /// <param name="r">
        /// The red component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="g">
        /// The green component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="b">
        /// The blue component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="a">
        /// The alpha (transparency) component of the color to be used for drawing (0-255).
        /// </param>
        /// <param name="thickness">
        /// The thickness of the lines used to draw the detection results.
        /// </param>
        public void DrawDetectResult<T>(T[] array, int width, int height, int bytesPerPixel, bool flip, int r, int g, int b, int a, int thickness) where T : unmanaged
        {
            if (array == null)
                throw new ArgumentNullException("array");
            ThrowIfDisposed();

#if !DLIB_DONT_USE_UNSAFE_CODE
            unsafe
            {
                fixed (T* ptr = array)
                {
                    DlibFaceLandmarkDetector_DrawDetectResult(nativeObj, (IntPtr)ptr, width, height, bytesPerPixel, flip, r, g, b, a, thickness);
                }
            }
#else
            GCHandle arrayHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
            DlibFaceLandmarkDetector_DrawDetectResult(nativeObj, arrayHandle.AddrOfPinnedObject(), width, height, bytesPerPixel, flip, r, g, b, a, thickness);
            arrayHandle.Free();
#endif
        }

        /// <summary>
        /// Draws the detected landmark result on the provided texture.
        /// </summary>
        /// <param name="texture2D">
        /// The <see cref="Texture2D"/> on where the detected landmark result will be drawn. 
        /// Processing speed is fastest when the texture format is <see cref="TextureFormat.RGBA32"/>, <see cref="TextureFormat.RGB24"/>, or <see cref="TextureFormat.Alpha8"/>.
        /// </param>
        /// <param name="r">
        /// The red component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="g">
        /// The green component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="b">
        /// The blue component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="a">
        /// The alpha (transparency) component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="updateMipmaps">
        /// If set to true, mipmap levels of the texture will be recalculated.
        /// </param>
        /// <param name="makeNoLongerReadable">
        /// If set to true, the system memory copy of the texture will be released, making it no longer readable.
        /// </param>
        public void DrawDetectLandmarkResult(Texture2D texture2D, int r, int g, int b, int a, bool updateMipmaps = false, bool makeNoLongerReadable = false)
        {
            DrawDetectLandmarkResult(texture2D, r, g, b, a, null, null, updateMipmaps, makeNoLongerReadable);
        }

        /// <summary>
        /// Draws the detected landmark result on the provided texture.
        /// </summary>
        /// <param name="texture2D">
        /// The <see cref="Texture2D"/> on where the detected landmark result will be drawn. 
        /// Processing speed is fastest when the texture format is <see cref="TextureFormat.RGBA32"/>, <see cref="TextureFormat.RGB24"/>, or <see cref="TextureFormat.Alpha8"/>.
        /// </param>
        /// <param name="r">
        /// The red component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="g">
        /// The green component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="b">
        /// The blue component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="a">
        /// The alpha (transparency) component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="pixels32Buffer">
        /// An optional array to receive pixel data in the form of Color32. You can pass in an existing array to avoid allocating new memory each frame. 
        /// The array should be initialized to a length matching the width * height of the texture.
        /// <a href="http://docs.unity3d.com/ScriptReference/WebCamTexture.GetPixels32.html">WebCamTexture.GetPixels32</a>
        /// </param>
        /// <param name="rawTextureDataBuffer">
        /// An optional array to receive raw texture data in the form of bytes. You can pass in an existing array to avoid allocating new memory each frame. 
        /// The array should be initialized to a length matching the raw data size of the texture.
        /// <a href="https://docs.unity3d.com/ScriptReference/Texture2D.GetRawTextureData.html">Texture2D.GetRawTextureData</a>
        /// </param>
        /// <param name="updateMipmaps">
        /// If set to true, mipmap levels of the texture will be recalculated.
        /// </param>
        /// <param name="makeNoLongerReadable">
        /// If set to true, the system memory copy of the texture will be released, making it no longer readable.
        /// </param>
        public void DrawDetectLandmarkResult(Texture2D texture2D, int r, int g, int b, int a, Color32[] pixels32Buffer, byte[] rawTextureDataBuffer = null, bool updateMipmaps = false, bool makeNoLongerReadable = false)
        {
            if (texture2D == null)
                throw new ArgumentNullException("texture2D");
            ThrowIfDisposed();

            TextureFormat format = texture2D.format;
            if (format == TextureFormat.RGBA32 || format == TextureFormat.RGB24 || format == TextureFormat.Alpha8)
            {
#if !DLIB_DONT_USE_UNSAFE_CODE
                unsafe
                {
                    NativeArray<byte> rawTextureData = texture2D.GetRawTextureData<byte>();

                    DlibFaceLandmarkDetector_DrawDetectLandmarkResult(nativeObj, (IntPtr)NativeArrayUnsafeUtility.GetUnsafeReadOnlyPtr(rawTextureData), texture2D.width, texture2D.height, (int)texture2D.format, true, r, g, b, a);

                }
                texture2D.Apply(updateMipmaps, makeNoLongerReadable);

                return;
#else
                if ((rawTextureDataBuffer != null) || (pixels32Buffer == null && texture2D.mipmapCount == 1))
                {
                    GCHandle rawTextureDataHandle;
                    if (rawTextureDataBuffer == null)
                    {
                        byte[] rawTextureData = texture2D.GetRawTextureData();
                        rawTextureDataHandle = GCHandle.Alloc(rawTextureData, GCHandleType.Pinned);
                        DlibFaceLandmarkDetector_DrawDetectLandmarkResult(nativeObj, rawTextureDataHandle.AddrOfPinnedObject(), texture2D.width, texture2D.height, (int)texture2D.format, true, r, g, b, a);
                        texture2D.LoadRawTextureData(rawTextureDataHandle.AddrOfPinnedObject(), rawTextureData.Length);
                    }
                    else
                    {
                        rawTextureDataHandle = GCHandle.Alloc(rawTextureDataBuffer, GCHandleType.Pinned);
                        DlibFaceLandmarkDetector_DrawDetectLandmarkResult(nativeObj, rawTextureDataHandle.AddrOfPinnedObject(), texture2D.width, texture2D.height, (int)texture2D.format, true, r, g, b, a);
                        texture2D.LoadRawTextureData(rawTextureDataHandle.AddrOfPinnedObject(), rawTextureDataBuffer.Length);
                    }
                    texture2D.Apply(updateMipmaps, makeNoLongerReadable);
                    rawTextureDataHandle.Free();

                    return;
                }
#endif
            }

            //You can use SetPixels32 with the following texture formats:
            //https://docs.unity3d.com/2020.3/Documentation/ScriptReference/Texture2D.SetPixels32.html
#if !DLIB_DONT_USE_UNSAFE_CODE
            if (pixels32Buffer == null)
            {
                Color32[] pixels32 = texture2D.GetPixels32();

                unsafe
                {
                    fixed (Color32* ptr = pixels32)
                    {
                        DlibFaceLandmarkDetector_DrawDetectLandmarkResult(nativeObj, (IntPtr)ptr, texture2D.width, texture2D.height, 4, true, r, g, b, a);
                    }
                }
                texture2D.SetPixels32(pixels32);
            }
            else
            {
                unsafe
                {
                    fixed (Color32* ptr = pixels32Buffer)
                    {
                        DlibFaceLandmarkDetector_DrawDetectLandmarkResult(nativeObj, (IntPtr)ptr, texture2D.width, texture2D.height, 4, true, r, g, b, a);
                    }
                }
                texture2D.SetPixels32(pixels32Buffer);
            }
            texture2D.Apply(updateMipmaps, makeNoLongerReadable);
#else
            GCHandle pixels32Handle;
            if (pixels32Buffer == null)
            {
                Color32[] pixels32 = texture2D.GetPixels32();

                pixels32Handle = GCHandle.Alloc(pixels32, GCHandleType.Pinned);
                DlibFaceLandmarkDetector_DrawDetectLandmarkResult(nativeObj, pixels32Handle.AddrOfPinnedObject(), texture2D.width, texture2D.height, 4, true, r, g, b, a);

                texture2D.SetPixels32(pixels32);
            }
            else
            {
                pixels32Handle = GCHandle.Alloc(pixels32Buffer, GCHandleType.Pinned);
                DlibFaceLandmarkDetector_DrawDetectLandmarkResult(nativeObj, pixels32Handle.AddrOfPinnedObject(), texture2D.width, texture2D.height, 4, true, r, g, b, a);

                texture2D.SetPixels32(pixels32Buffer);
            }
            texture2D.Apply(updateMipmaps, makeNoLongerReadable);
            pixels32Handle.Free();
#endif

        }

        /// <summary>
        /// Draws the detected landmark result on the provided image buffer.
        /// </summary>
        /// <param name="intPtr">
        /// The pointer to the memory buffer where the detected landmark result will be drawn. 
        /// </param>
        /// <param name="width">
        /// The width of the image (in pixels) where the landmarks will be drawn.
        /// </param>
        /// <param name="height">
        /// The height of the image (in pixels) where the landmarks will be drawn.
        /// </param>
        /// <param name="bytesPerPixel">
        /// The number of bytes per pixel in the memory buffer. Valid values are 1 (grayscale), 3 (RGB), or 4 (RGBA).
        /// </param>
        /// <param name="r">
        /// The red component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="g">
        /// The green component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="b">
        /// The blue component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="a">
        /// The alpha (transparency) component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        public void DrawDetectLandmarkResult(IntPtr intPtr, int width, int height, int bytesPerPixel, int r, int g, int b, int a)
        {
            DrawDetectLandmarkResult(intPtr, width, height, bytesPerPixel, false, r, g, b, a);
        }

        /// <summary>
        /// Draws the detected landmark result on the provided image buffer with optional vertical flipping.
        /// </summary>
        /// <param name="intPtr">
        /// The pointer to the memory buffer where the detected landmark result will be drawn. 
        /// </param>
        /// <param name="width">
        /// The width of the image (in pixels) where the landmarks will be drawn.
        /// </param>
        /// <param name="height">
        /// The height of the image (in pixels) where the landmarks will be drawn.
        /// </param>
        /// <param name="bytesPerPixel">
        /// The number of bytes per pixel in the memory buffer. Valid values are 1 (grayscale), 3 (RGB), or 4 (RGBA).
        /// </param>
        /// <param name="flip">
        /// A boolean flag indicating whether to flip the image vertically. 
        /// Set to <c>true</c> to flip, or <c>false</c> to leave the image unflipped.
        /// </param>
        /// <param name="r">
        /// The red component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="g">
        /// The green component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="b">
        /// The blue component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="a">
        /// The alpha (transparency) component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        public void DrawDetectLandmarkResult(IntPtr intPtr, int width, int height, int bytesPerPixel, bool flip, int r, int g, int b, int a)
        {
            if (intPtr == IntPtr.Zero)
                throw new ArgumentException("intPtr == IntPtr.Zero");
            ThrowIfDisposed();

            DlibFaceLandmarkDetector_DrawDetectLandmarkResult(nativeObj, intPtr, width, height, bytesPerPixel, flip, r, g, b, a);
        }

        /// <summary>
        /// Draws the detected landmark result on the provided image buffer.
        /// </summary>
        /// <param name="array">
        /// The array representing the image data where the detected landmark result will be drawn.
        /// The array should contain pixel data in the format corresponding to <paramref name="bytesPerPixel"/>.
        /// </param>
        /// <param name="width">
        /// The width of the image (in pixels) where the landmarks will be drawn.
        /// </param>
        /// <param name="height">
        /// The height of the image (in pixels) where the landmarks will be drawn.
        /// </param>
        /// <param name="bytesPerPixel">
        /// The number of bytes per pixel in the memory buffer. Valid values are 1 (grayscale), 3 (RGB), or 4 (RGBA).
        /// </param>
        /// <param name="r">
        /// The red component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="g">
        /// The green component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="b">
        /// The blue component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="a">
        /// The alpha (transparency) component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        public void DrawDetectLandmarkResult<T>(T[] array, int width, int height, int bytesPerPixel, int r, int g, int b, int a) where T : unmanaged
        {
            DrawDetectLandmarkResult<T>(array, width, height, bytesPerPixel, false, r, g, b, a);
        }

        /// <summary>
        /// Draws the detected landmark result on the provided image buffer, with an option to flip the image vertically.
        /// </summary>
        /// <param name="array">
        /// The array representing the image data where the detected landmark result will be drawn.
        /// The array should contain pixel data in the format corresponding to <paramref name="bytesPerPixel"/>.
        /// </param>
        /// <param name="width">
        /// The width of the image (in pixels) where the landmarks will be drawn.
        /// </param>
        /// <param name="height">
        /// The height of the image (in pixels) where the landmarks will be drawn.
        /// </param>
        /// <param name="bytesPerPixel">
        /// The number of bytes per pixel in the memory buffer. Valid values are 1 (grayscale), 3 (RGB), or 4 (RGBA).
        /// </param>
        /// <param name="flip">
        /// A boolean flag indicating whether to flip the image vertically. 
        /// Set to <c>true</c> to flip, or <c>false</c> to leave the image unflipped.
        /// </param>
        /// <param name="r">
        /// The red component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="g">
        /// The green component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="b">
        /// The blue component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        /// <param name="a">
        /// The alpha (transparency) component (0-255) of the color to use for drawing the landmarks.
        /// </param>
        public void DrawDetectLandmarkResult<T>(T[] array, int width, int height, int bytesPerPixel, bool flip, int r, int g, int b, int a) where T : unmanaged
        {
            if (array == null)
                throw new ArgumentNullException("array");
            ThrowIfDisposed();

#if !DLIB_DONT_USE_UNSAFE_CODE 
            unsafe
            {
                fixed (T* ptr = array)
                {
                    DlibFaceLandmarkDetector_DrawDetectLandmarkResult(nativeObj, (IntPtr)ptr, width, height, bytesPerPixel, flip, r, g, b, a);
                }
            }
#else
            GCHandle arrayHandle = GCHandle.Alloc(array, GCHandleType.Pinned);
            DlibFaceLandmarkDetector_DrawDetectLandmarkResult(nativeObj, arrayHandle.AddrOfPinnedObject(), width, height, bytesPerPixel, flip, r, g, b, a);
            arrayHandle.Free();
#endif
        }


#if (UNITY_IOS || UNITY_WEBGL) && !UNITY_EDITOR
        const string LIBNAME = "__Internal";
#else
        const string LIBNAME = "dlibfacelandmarkdetector";
#endif

        [DllImport(LIBNAME)]
        private static extern IntPtr DlibFaceLandmarkDetector_Init();

        [DllImport(LIBNAME)]
        private static extern void DlibFaceLandmarkDetector_Dispose(IntPtr nativeObj);


        [DllImport(LIBNAME, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool DlibFaceLandmarkDetector_LoadObjectDetector(IntPtr self, [MarshalAs(UnmanagedType.LPWStr)] string objectDetectorFilename);

        [DllImport(LIBNAME, CharSet = CharSet.Unicode)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool DlibFaceLandmarkDetector_LoadShapePredictor(IntPtr self, [MarshalAs(UnmanagedType.LPWStr)] string shapePredictorFilename);

        [DllImport(LIBNAME)]
        private static extern void DlibFaceLandmarkDetector_SetImage(IntPtr self, IntPtr byteArray, int texWidth, int texHeight, int bytesPerPixel, [MarshalAs(UnmanagedType.U1)] bool flip);

        [DllImport(LIBNAME)]
        private static extern int DlibFaceLandmarkDetector_Detect(IntPtr self, double adjust_threshold);

        [DllImport(LIBNAME)]
        private static extern void DlibFaceLandmarkDetector_GetDetectResult(IntPtr self, [Out] double[] result);

        [DllImport(LIBNAME)]
        private static extern void DlibFaceLandmarkDetector_GetDetectResult(IntPtr self, [Out] IntPtr result);

        [DllImport(LIBNAME)]
        private static extern int DlibFaceLandmarkDetector_DetectLandmark(IntPtr self, double left, double top, double width, double height);

        [DllImport(LIBNAME)]
        private static extern void DlibFaceLandmarkDetector_GetDetectLandmarkResult(IntPtr self, [Out] double[] result);

        [DllImport(LIBNAME)]
        private static extern void DlibFaceLandmarkDetector_GetDetectLandmarkResult(IntPtr self, [Out] IntPtr result);

        [DllImport(LIBNAME)]
        [return: MarshalAs(UnmanagedType.U1)]
        private static extern bool DlibFaceLandmarkDetector_IsAllPartsInRect(IntPtr self);

        [DllImport(LIBNAME)]
        private static extern long DlibFaceLandmarkDetector_ShapePredictorNumParts(IntPtr self);

        [DllImport(LIBNAME)]
        private static extern long DlibFaceLandmarkDetector_ShapePredictorNumFeatures(IntPtr self);

        [DllImport(LIBNAME)]
        private static extern void DlibFaceLandmarkDetector_DrawDetectResult(IntPtr self, IntPtr byteArray, int texWidth, int texHeight, int bytesPerPixel, [MarshalAs(UnmanagedType.U1)] bool flip, int r, int g, int b, int a, int thickness);

        [DllImport(LIBNAME)]
        private static extern void DlibFaceLandmarkDetector_DrawDetectLandmarkResult(IntPtr self, IntPtr byteArray, int texWidth, int texHeight, int bytesPerPixel, [MarshalAs(UnmanagedType.U1)] bool flip, int r, int g, int b, int a);
    }
}
#pragma warning restore 0219