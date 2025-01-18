using UnityEngine;
using OpenCvSharp;

public class FaceDector : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    WebCamTexture webCamTexture;
    CascadeClassifier cascade;
    OpenCvSharp.Rect playerFace;
    void Start()
    {
        WebCamDevice[] devices = WebCamTexture.devices;

        webCamTexture = new WebCamTexture(devices[0].name);
        webCamTexture.Play();
        cascade = new CascadeClassifier(@"Assets\OpenCV+Unity\Demo\Face_Detector\haarcascade_frontalface_default.xml");
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
            //Debug.Log(faces[0].Location);
            playerFace = faces[0];
        }
    }

    void display(Mat frame)
    {
        if (playerFace != null) 
        {
            frame.Rectangle(playerFace, new Scalar(250, 0, 0), 2);
        }

        Texture newTexture = OpenCvSharp.Unity.MatToTexture(frame);
        GetComponent<Renderer>().material.mainTexture = newTexture;
    }
}
