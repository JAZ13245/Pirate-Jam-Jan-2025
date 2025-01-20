using UnityEngine;
using OpenCvSharp;

public class FaceDector : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    WebCamTexture webCamTexture;
    CascadeClassifier cascade;
    OpenCvSharp.Rect eye1;
    OpenCvSharp.Rect eye2;
    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        webCamTexture = new WebCamTexture(devices[0].name);
        webCamTexture.Play();
        cascade = new CascadeClassifier(@"Assets\OpenCV+Unity\Demo\Face_Detector\haarcascade_frontalface_default.xml");
        //cascade = new CascadeClassifier(@"Assets\OpenCV+Unity\Demo\Face_Detector\haarcascade_eye.xml");
    }

    // Update is called once per frame
    void Update()
    {
        //GetComponent<Renderer>().material.mainTexture = webCamTexture;
        Mat frame = OpenCvSharp.Unity.TextureToMat(webCamTexture);

        findNewFace(frame);
        display(frame);
    }

    void findNewFace(Mat frame) 
    { 
        var faces = cascade.DetectMultiScale(frame, 1.1, 2, HaarDetectionType.ScaleImage);

        if(faces.Length >= 1)
        {
            //Debug.Log(faces.Length);
            eye1 = faces[0];
            //eye2 = faces[1];
        }
    }

    void display(Mat frame)
    {
        if (eye1 != null) 
        {
            frame.Rectangle(eye1, new Scalar(250, 0, 0), 2);
        }
        if (eye2 != null) 
        {
            //frame.Rectangle(eye2, new Scalar(250, 0, 0), 2);
        }

        Texture newTexture = OpenCvSharp.Unity.MatToTexture(frame);
        GetComponent<Renderer>().material.mainTexture = newTexture;
    }
}
