using UnityEngine;
using ZXing;
using TMPro;
using UnityEngine.UI;

public class QrCode : MonoBehaviour
{
    [SerializeField]
    private RawImage _rawImageBackground;

    [SerializeField]
    private AspectRatioFitter _aspectRatioFitter;

    [SerializeField]
    private TextMeshProUGUI _textOut;

    [SerializeField]
    private RectTransform _scanZone;

    private bool _isCamAvailable;
    private WebCamTexture _webCamTexture;

    void Start()
    {
        SetupCamera();
    }

    void Update()
    {
        UpdateCameraRender();

        if (_isCamAvailable && _webCamTexture.width > 100)
        {
            Scan();
        }
    }

    private void SetupCamera()
    {
        WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length == 0)
        {
            _isCamAvailable = false;
            return;
        }

        WebCamDevice bestDevice = devices[0]; 
        int bestResolution = 0;

        foreach (var device in devices)
        {
            WebCamTexture testCam = new WebCamTexture(device.name);
            testCam.Play(); 
            int resolution = testCam.width * testCam.height;
            testCam.Stop();

        
            if (resolution > bestResolution)
            {
                bestResolution = resolution;
                bestDevice = device;
            }
        }

        _webCamTexture = new WebCamTexture(bestDevice.name, (int)_scanZone.rect.width, (int)_scanZone.rect.height);
        _webCamTexture.Play();
        _rawImageBackground.texture = _webCamTexture;
        _isCamAvailable = true;
    }


    private void UpdateCameraRender()
    {
        if (!_isCamAvailable)
        {
            return;
        }

        float ratio = (float)_webCamTexture.width / (float)_webCamTexture.height;
        _aspectRatioFitter.aspectRatio = ratio;

        int orientation = -_webCamTexture.videoRotationAngle;
        bool mirrored = _webCamTexture.videoVerticallyMirrored;

        if (mirrored)
        {
            _rawImageBackground.rectTransform.localEulerAngles = new Vector3(0, 180, orientation);
        }
        else
        {
            _rawImageBackground.rectTransform.localEulerAngles = new Vector3(0, 0, orientation);
        }
    }

    private void Scan()
    {
        try
        {
            IBarcodeReader barcodeReader = new BarcodeReader();
            Result result = barcodeReader.Decode(_webCamTexture.GetPixels32(), _webCamTexture.width, _webCamTexture.height);

            if (result != null)
            {
                _textOut.text = result.Text;
            }
        }
        catch
        {
            _textOut.text = "FAILED TO READ QR CODE";
        }
    }
}
