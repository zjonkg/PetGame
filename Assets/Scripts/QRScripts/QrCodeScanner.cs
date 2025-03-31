using UnityEngine;
using ZXing;
using TMPro;
using UnityEngine.UI;
using ZXing.QrCode.Internal;

public class QrCode : MonoBehaviour
{
    [SerializeField] private RawImage _rawImageBackground;
    [SerializeField] private AspectRatioFitter _aspectRatioFitter;
    [SerializeField] private TextMeshProUGUI _textOut;
    [SerializeField] private RectTransform _scanZone;
    [SerializeField] private Button _flashButton;
    [SerializeField] private ProcessQRController processQRController;

    private bool _isCamAvailable;
    private WebCamTexture _webCamTexture;
    private bool _isFlashOn = false;
    private AndroidJavaObject _camera;
    private bool _scanned = false;

    void Start()
    {
        SetupCamera();
        _flashButton.onClick.AddListener(ToggleFlash);
    }

    void Update()
    {
        if (!_isCamAvailable || _scanned) return;
        UpdateCameraRender();
        if (_webCamTexture.width > 100)
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
        if (!_isCamAvailable) return;

        float ratio = (float)_webCamTexture.width / _webCamTexture.height;
        _aspectRatioFitter.aspectRatio = ratio;

        int orientation = -_webCamTexture.videoRotationAngle;
        bool mirrored = _webCamTexture.videoVerticallyMirrored;

        _rawImageBackground.rectTransform.localEulerAngles = new Vector3(0, mirrored ? 180 : 0, orientation);
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
                processQRController.ValidateQRCodeFromExternal(result.Text, "1");
                _scanned = true;
                StopCamera();
            }
        }
        catch
        {
            _textOut.text = "FAILED TO READ QR CODE";
        }
    }

    private void StopCamera()
    {
        if (_webCamTexture != null)
        {
            _webCamTexture.Stop();
            _isCamAvailable = false;
        }
    }

    private void ToggleFlash()
    {
#if UNITY_ANDROID
        try
        {
            if (_camera == null)
            {
                AndroidJavaClass cameraClass = new AndroidJavaClass("android.hardware.Camera");
                _camera = cameraClass.CallStatic<AndroidJavaObject>("open", 0);
            }

            if (_isFlashOn)
            {
                AndroidJavaObject parameters = _camera.Call<AndroidJavaObject>("getParameters");
                parameters.Call("setFlashMode", "off");
                _camera.Call("setParameters", parameters);
                _camera.Call("stopPreview");
            }
            else
            {
                AndroidJavaObject parameters = _camera.Call<AndroidJavaObject>("getParameters");
                parameters.Call("setFlashMode", "torch");
                _camera.Call("setParameters", parameters);
                _camera.Call("startPreview");
            }

            _isFlashOn = !_isFlashOn;
        }
        catch
        {
            _textOut.text = "FLASH NOT SUPPORTED";
        }
#endif
    }

    private void OnDestroy()
    {
        if (_camera != null)
        {
            _camera.Call("release");
            _camera = null;
        }
    }
}
