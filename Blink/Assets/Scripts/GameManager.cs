using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DlibFaceLandmarkDetector;
using DlibFaceLandmarkDetector.UnityUtils;
using DlibFaceLandmarkDetectorExample;
using System.Threading;
using System;
using System.IO;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Output")]
    /// <summary>
    /// The RawImage for previewing the result.
    /// </summary>
    public RawImage resultPreview;

    private static GameManager _instance;

    public static GameManager Instance { get { return _instance; } }

    private bool faceDetectionEnable = true;
    public bool GetFaceCamEnable() { return faceDetectionEnable; }

    public List<Vector2> points;

    /// Set the name of the device to use.
    [SerializeField, TooltipAttribute("Set the name of the device to use.")]
    public string requestedDeviceName = null;

    /// The face landmark detector.
    FaceLandmarkDetector faceLandmarkDetector;

    /// The dlib shape predictor file name.
    string dlibShapePredictorFileName = "DlibFaceLandmarkDetector/sp_human_face_68.dat";

    /// The dlib shape predictor file path.
    string dlibShapePredictorFilePath;

    /// The CancellationTokenSource.
    CancellationTokenSource cts = new CancellationTokenSource();

    /// The webcam texture.
    WebCamTexture webCamTexture;

    /// The webcam device.
    WebCamDevice webCamDevice;

    /// The colors.
    Color32[] colors;

    /// The rotated colors.
    Color32[] rotatedColors;

    /// Set the width of WebCamTexture.
    [SerializeField, TooltipAttribute("Set the width of WebCamTexture.")]
    public int requestedWidth = 320;

    /// Set the height of WebCamTexture.
    [SerializeField, TooltipAttribute("Set the height of WebCamTexture.")]
    public int requestedHeight = 240;


    /// Set FPS of WebCamTexture.
    [SerializeField, TooltipAttribute("Set FPS of WebCamTexture.")]
    public int requestedFPS = 30;

    /// Set whether to use the front facing camera.
    [SerializeField, TooltipAttribute("Set whether to use the front facing camera.")]
    public bool requestedIsFrontFacing = false;

    /// The texture.
    Texture2D texture;

    /// The texture.
    [SerializeField] private Texture2D defaultTexture;

    /// Indicates whether this instance is waiting for initialization to complete.
    bool isInitWaiting = false;

    /// Indicates whether this instance has been initialized.
    bool hasInitDone = false;

    /// Determines if rotates 90 degree.
    bool rotate90Degree = false;

    /// The screenOrientation.
    ScreenOrientation screenOrientation;

    /// The width of the screen.
    int screenWidth;

    /// The height of the screen.
    int screenHeight;

    /// Determines if adjust pixels direction.
    [SerializeField, TooltipAttribute("Determines if adjust pixels direction.")]
    public bool adjustPixelsDirection = false;

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;

        //DontDestroyOnLoad(this.gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        points = new List<Vector2>();
        dlibShapePredictorFileName = DlibFaceLandmarkDetectorExample.DlibFaceLandmarkDetectorExample.dlibShapePredictorFileName;
        dlibShapePredictorFilePath = await Utils.getFilePathAsyncTask(dlibShapePredictorFileName, cancellationToken: cts.Token);

        Run();
    }


    // Update is called once per frame
    private void Update()
    {
        // Blink
        Color32[] colors = GetColors();

        if (colors != null)
        {
            faceLandmarkDetector.SetImage<Color32>(colors, texture.width, texture.height, 4, true);

            List<UnityEngine.Rect> detectResult = faceLandmarkDetector.Detect();

            if (detectResult.Count > 0)
            {
                points = faceLandmarkDetector.DetectLandmark(detectResult[0]);
            }

            faceLandmarkDetector.DrawDetectLandmarkResult<Color32>(colors, texture.width, texture.height, 4, true, 0, 255, 0, 255);
            texture.SetPixels32(colors);
            texture.Apply(false);
        }
    }

    private void Run()
    {
        if (string.IsNullOrEmpty(dlibShapePredictorFilePath))
        {
            Debug.LogError("shape predictor file does not exist. Please copy from ?DlibFaceLandmarkDetector/StreamingAssets/DlibFaceLandmarkDetector/? to ?Assets/StreamingAssets/DlibFaceLandmarkDetector/? folder. ");
        }

        faceLandmarkDetector = new FaceLandmarkDetector(dlibShapePredictorFilePath);

        Initialize();

        if (faceLandmarkDetector.GetShapePredictorNumParts() != 68)
            Debug.LogWarning("The DrawDetectLandmarkResult method does not support ShapePredictorNumParts sizes other than 68 points, so the drawing will be incorrect."
                + " If you want to draw the result correctly, we recommend using the OpenCVForUnityUtils.DrawFaceLandmark method.");
    }

    // <summary>
    /// Initializes webcam texture.
    /// </summary>
    private void Initialize()
    {
        if (isInitWaiting)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
            // Set the requestedFPS parameter to avoid the problem of the WebCamTexture image becoming low light on some Android devices. (Pixel, pixel 2)
            // https://forum.unity.com/threads/android-webcamtexture-in-low-light-only-some-models.520656/
            // https://forum.unity.com/threads/released-opencv-for-unity.277080/page-33#post-3445178
            if (requestedIsFrontFacing)
            {
                int rearCameraFPS = requestedFPS;
                requestedFPS = 15;
                StartCoroutine(_Initialize());
                requestedFPS = rearCameraFPS;
            }
            else
            {
                StartCoroutine(_Initialize());
            }
#else
        StartCoroutine(_Initialize());
#endif
    }

    /// <summary>
    /// Initializes webcam texture by coroutine.
    /// </summary>
    private IEnumerator _Initialize()
    {
        if (hasInitDone)
            Dispose();

        isInitWaiting = true;

        // Creates a WebCamTexture with settings closest to the requested name, resolution, and frame rate.
        var devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            Debug.LogError("Camera device does not exist.");
            isInitWaiting = false;
            yield break;
        }

        if (!String.IsNullOrEmpty(requestedDeviceName))
        {
            // Try to parse requestedDeviceName as an index
            int requestedDeviceIndex = -1;
            if (Int32.TryParse(requestedDeviceName, out requestedDeviceIndex))
            {
                if (requestedDeviceIndex >= 0 && requestedDeviceIndex < devices.Length)
                {
                    webCamDevice = devices[requestedDeviceIndex];
                    webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight, requestedFPS);
                }
            }
            else
            {
                // Search for a device with a matching name
                for (int cameraIndex = 0; cameraIndex < devices.Length; cameraIndex++)
                {
                    if (devices[cameraIndex].name == requestedDeviceName)
                    {
                        webCamDevice = devices[cameraIndex];
                        webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight, requestedFPS);
                        break;
                    }
                }
            }
            if (webCamTexture == null)
                Debug.Log("Cannot find camera device " + requestedDeviceName + ".");
        }

        if (webCamTexture == null)
        {
            var prioritizedKinds = new WebCamKind[]
            {
                    WebCamKind.WideAngle,
                    WebCamKind.Telephoto,
                    WebCamKind.UltraWideAngle,
                    WebCamKind.ColorAndDepth
            };

            // Checks how many and which cameras are available on the device
            foreach (var kind in prioritizedKinds)
            {
                foreach (var device in devices)
                {
                    if (device.kind == kind && device.isFrontFacing == requestedIsFrontFacing)
                    {
                        webCamDevice = device;
                        webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight, requestedFPS);
                        break;
                    }
                }
                if (webCamTexture != null) break;
            }
        }

        if (webCamTexture == null)
        {
            webCamDevice = devices[0];
            webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight, requestedFPS);
        }

        // Starts the camera
        webCamTexture.Play();

        while (true)
        {
            if (webCamTexture.didUpdateThisFrame)
            {
                //Debug.Log("name:" + webCamTexture.deviceName + " width:" + webCamTexture.width + " height:" + webCamTexture.height + " fps:" + webCamTexture.requestedFPS);
                //Debug.Log("videoRotationAngle:" + webCamTexture.videoRotationAngle + " videoVerticallyMirrored:" + webCamTexture.videoVerticallyMirrored + " isFrongFacing:" + webCamDevice.isFrontFacing);

                screenOrientation = Screen.orientation;
                screenWidth = Screen.width;
                screenHeight = Screen.height;
                isInitWaiting = false;
                hasInitDone = true;

                OnInited();

                break;
            }
            else
            {
                yield return 0;
            }
        }
    }

    /// <summary>
    /// Releases all resource.
    /// </summary>
    private void Dispose()
    {
        rotate90Degree = false;
        isInitWaiting = false;
        hasInitDone = false;

        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            WebCamTexture.Destroy(webCamTexture);
            webCamTexture = null;
        }
        if (texture != null)
        {
            Texture2D.Destroy(texture);
            texture = null;
        }
    }

    /// <summary>
    /// Raises the webcam texture initialized event.
    /// </summary>
    private void OnInited()
    {
        if (colors == null || colors.Length != webCamTexture.width * webCamTexture.height)
        {
            colors = new Color32[webCamTexture.width * webCamTexture.height];
            rotatedColors = new Color32[webCamTexture.width * webCamTexture.height];
        }

        if (rotate90Degree)
        {
            texture = new Texture2D(webCamTexture.height, webCamTexture.width, TextureFormat.RGBA32, false);
        }
        else
        {
            texture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);
        }

        if(resultPreview != null)
        {
            if (faceDetectionEnable)
            {
                resultPreview.texture = texture;
                resultPreview.GetComponent<AspectRatioFitter>().aspectRatio = (float)texture.width / texture.height;
            }
            else
            {
                resultPreview.texture = defaultTexture;
            }
        }
    }

    /// <summary>
    /// Gets the current WebCameraTexture frame that converted to the correct direction.
    /// </summary>
    private Color32[] GetColors()
    {
        if (webCamTexture != null)
            webCamTexture.GetPixels32(colors);

        if (adjustPixelsDirection)
        {
            //Adjust an array of color pixels according to screen orientation and WebCamDevice parameter.
            if (rotate90Degree)
            {
                Rotate90CW(colors, rotatedColors, webCamTexture.width, webCamTexture.height);
                FlipColors(rotatedColors, webCamTexture.width, webCamTexture.height);
                return rotatedColors;
            }
            else
            {
                FlipColors(colors, webCamTexture.width, webCamTexture.height);
                return colors;
            }
        }
        return colors;
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    private void OnDestroy()
    {
        Dispose();

        if (faceLandmarkDetector != null)
            faceLandmarkDetector.Dispose();

        if (cts != null)
            cts.Dispose();
    }

    /// <summary>
    /// Rotates 90 degrees (CLOCKWISE).
    /// </summary>
    /// <param name="src">Src colors.</param>
    /// <param name="dst">Dst colors.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    private void Rotate90CW(Color32[] src, Color32[] dst, int height, int width)
    {
        int i = 0;
        for (int x = height - 1; x >= 0; x--)
        {
            for (int y = 0; y < width; y++)
            {
                dst[i] = src[x + y * height];
                i++;
            }
        }
    }

    /// <summary>
    /// Flips the colors.
    /// </summary>
    /// <param name="colors">Colors.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    private void FlipColors(Color32[] colors, int width, int height)
    {
        int flipCode = int.MinValue;

        if (webCamDevice.isFrontFacing)
        {
            if (webCamTexture.videoRotationAngle == 0)
            {
                flipCode = 1;
            }
            else if (webCamTexture.videoRotationAngle == 90)
            {
                flipCode = 1;
            }
            if (webCamTexture.videoRotationAngle == 180)
            {
                flipCode = 0;
            }
            else if (webCamTexture.videoRotationAngle == 270)
            {
                flipCode = 0;
            }
        }
        else
        {
            if (webCamTexture.videoRotationAngle == 180)
            {
                flipCode = -1;
            }
            else if (webCamTexture.videoRotationAngle == 270)
            {
                flipCode = -1;
            }
        }

        if (flipCode > int.MinValue)
        {
            if (rotate90Degree)
            {
                if (flipCode == 0)
                {
                    FlipVertical(colors, colors, height, width);
                }
                else if (flipCode == 1)
                {
                    FlipHorizontal(colors, colors, height, width);
                }
                else if (flipCode < 0)
                {
                    Rotate180(colors, colors, height, width);
                }
            }
            else
            {
                if (flipCode == 0)
                {
                    FlipVertical(colors, colors, width, height);
                }
                else if (flipCode == 1)
                {
                    FlipHorizontal(colors, colors, width, height);
                }
                else if (flipCode < 0)
                {
                    Rotate180(colors, colors, height, width);
                }
            }
        }
    }

    /// <summary>
    /// Flips vertical.
    /// </summary>
    /// <param name="src">Src colors.</param>
    /// <param name="dst">Dst colors.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    private void FlipVertical(Color32[] src, Color32[] dst, int width, int height)
    {
        for (var i = 0; i < height / 2; i++)
        {
            var y = i * width;
            var x = (height - i - 1) * width;
            for (var j = 0; j < width; j++)
            {
                int s = y + j;
                int t = x + j;
                Color32 c = src[s];
                dst[s] = src[t];
                dst[t] = c;
            }
        }
    }

    /// <summary>
    /// Flips horizontal.
    /// </summary>
    /// <param name="src">Src colors.</param>
    /// <param name="dst">Dst colors.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    private void FlipHorizontal(Color32[] src, Color32[] dst, int width, int height)
    {
        for (int i = 0; i < height; i++)
        {
            int y = i * width;
            int x = y + width - 1;
            for (var j = 0; j < width / 2; j++)
            {
                int s = y + j;
                int t = x - j;
                Color32 c = src[s];
                dst[s] = src[t];
                dst[t] = c;
            }
        }
    }

    /// <summary>
    /// Rotates 180 degrees.
    /// </summary>
    /// <param name="src">Src colors.</param>
    /// <param name="dst">Dst colors.</param>
    /// <param name="width">Width.</param>
    /// <param name="height">Height.</param>
    private void Rotate180(Color32[] src, Color32[] dst, int height, int width)
    {
        int i = src.Length;
        for (int x = 0; x < i / 2; x++)
        {
            Color32 t = src[x];
            dst[x] = src[i - x - 1];
            dst[i - x - 1] = t;
        }
    }

    public void SetWebCamDevice(string webcam)
    {
        requestedDeviceName = webcam;
        StartCoroutine(_Initialize());
    }

    public void EnableWebCam(bool isEnable)
    {
        faceDetectionEnable = isEnable;
        StartCoroutine(_Initialize());
    }
}
