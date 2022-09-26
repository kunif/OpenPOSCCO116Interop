# OPOS version 1.16 コントロールをPOS for.NETから呼び出す代替手段

これはOPOS version 1.16 コントロールをPOS for.NETから呼び出すためのPOS for.NETのサービスオブジェクトです。  

POS for.NETには 既にLegacy COM Interopと呼ばれる、OPOSコントロールを呼び出す仕組みがPOS for.NET内部に組み込まれています。  
しかしこの仕組みには以下の課題があります。

- UnifiedPOS version 1.16で追加された以下の追加/変更に対応していません  
  - Lightsデバイスへパターン点灯/消灯,複数同時点灯機能を追加  
  - PosPowerデバイスへ秒単位のバッテリー状態通知関連プロパティ追加  
  - RCSD(リテイルコミュニケーションサービスデバイス)関連デバイス追加  
- UnifiedPOS version 1.15で追加された以下の追加/変更に対応していません  
  - ElectronicValueReader/WriterデバイスへCATデバイス機能を追加  
  - ElectronicValueReader/WriterデバイスのServiceTypeプロパティへEVRW_ST_CATを追加  
  - FiscalPrinterデバイスのCountryCode, DateTypeプロパティへ値を追加  
- UnifiedPOSに定義された36種類のデバイスクラスのうち24種類しかサポートされていません(v1.14.1の場合)  
  サポートされていないデバイス:  
  - Belt
  - Bill Acceptor
  - Bill Dispenser
  - Biometrics
  - Coin Acceptor
  - Electronic Journal
  - Electronic Value Reader/Writer
  - Gate
  - Image Scanner
  - Item Dispenser
  - Lights
  - RFID Scanner
- OPOSのBinaryConversionプロパティの値を変更するとOPOSを直接呼び出した場合とは違った変換処理が必要になります  

これらを解決するために、以下の特徴を持つサービスオブジェクトを作成しました。

- UnifiedPOS version 1.15/1.16で追加されたインタフェースや定義をサポートしたPOS for.NET用拡張DLLを組み込みました  
- UnifiedPOS version 1.16で追加されたデバイスをv1.14.1までの既存デバイスに見せかける抜け道でサポートしました  
- UnifiedPOSに定義された45種類のデバイスクラスをすべてサポートしました  
- OPOSのためのBinaryConversion処理は2種類に分けました  
  - POS for.NETでもOPOSでもstringのプロパティ/パラメータは何も処理せずそのまま通します  
  - POS for.NETでbyte[]やBitmap等のプロパティ/パラメータはBinaryConversionの値に応じた変換処理を行います  
- POS for.NETにおいてEnumとみなされているプロパティを読み取った際に、定義されていない値がOPOSから通知されたならば、PosControlExceptionを発生させ、その値を例外のErrorCodeExtendedプロパティに格納して通知します  
- 対応するOPOSデバイス名の情報は、POS for.NETのConfiguration.xmlファイルにて定義します  

## 開発/実行環境

このプログラムの開発および実行には以下が必要です。

- Visual Studio 2022またはVisual Studio Community 2022 version 17.3.4 (開発のみ)  
- .NET framework 4.8  
- Microsoft Point of Service for .NET v1.14.1 (POS for.NET) : https://www.microsoft.com/en-us/download/details.aspx?id=55758  
- OpenPOS.Extension.Ver115 : https://github.com/kunif/OpenPOSExtensionVer115  
- OpenPOS.Extension.Ver116 : https://github.com/kunif/OpenPOSExtensionVer116  
- Common Control Objects 1.16.000 : https://github.com/kunif/OPOS-CCO  
- 対象デバイスのOPOSサービスオブジェクト

このサービスオブジェクトの開発/実行には、Common Control Objects 1.16 の CCO と PIA および UnifiedPOS 1.15/1.16サポートPOS for.NET用拡張DLL が必要です。  
デバイスベンダのOPOSで対象デバイスの.ocxしかインストールされない場合や、CCOの 1.14.001およびそれ以前の版はサポートされないので入れ替えてください。  
Common Control Objects 1.16 の Install_CCOandPIA.bat にて CCO と PIA の両方をインストールしてください。  
UnifiedPOS 1.15/1.16サポートPOS for.NET用拡張DLLを登録してください。

## 実行環境へのインストール

実行環境へのインストールは以下の手順で行ってください。

- 適当なフォルダを作成し、 OpenPOS.CCO116Interop.dll をコピー  
  ドライブのルートではなく、かつフォルダのパス名は空白を含まず、0x7E以下の英数字のみで構成してください。  
  その方が問題発生の可能性が少ないでしょう。  

- POS for.NETレジストリのControlAssembliesキーに上記フォルダを値にして任意の名前で登録  
  例えば "AltCCOInterops"="C:\\\\POSforNET\\\\CCOInterop\\\\"  
  ただし、開発作業中はビルド時に処理の一環で自動的に登録されます。  
  対象キーの位置は以下です。  
  - 64bitOS: HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\POSfor.NET\\ControlAssemblies  
  - 32bitOS: HKEY_LOCAL_MACHINE\\SOFTWARE\\POSfor.NET\\ControlAssemblies  

## v1.16デバイスをv1.14.1でサポートする抜け道

v1.14.1自身でのILegacyControlObjectによるLegacy COM Interopの機能拡張がヒントとなり、v1.14.1に存在しない新しいデバイスクラスであっても、いったん共通のプロパティ/メソッド/イベントのうち同じものをサポートしている既存のデバイスとしてオブジェクトが作成できれば、新しいデバイスのインタフェース有無を確認することでv1.16の新デバイスとして呼び出しが出来そうだと思いつきました。
この仕組みはPOS for.NET自身のNativeなv1.16サービスオブジェクトを作成する場合にも使えるでしょう。

v1.16デバイスとv1.14.1デバイスの対応可能な関係を以下に示します。
なおPOS for.NETのオブジェクトとしては既存のv1.14.1デバイスとして扱いますが、OPOSのオブジェクトとしては対応する相応しいデバイスのCCOとレジストリを使います。

- VideoCapture  
  - ClearInputメソッド, ErrorEventイベントをサポートしていること  
  - このプロジェクトでは ImageScanner として実装  
  - 他には Biometrics, BumpBar, CheckScanner, ElectronicJournal, MICR, MSR, PINPad, PointCardRW, POSKeyboard, RemoteOrderDisplay, Scale, Scanner, SignatureCapture, SmartCardRW が使える  

- DeviceMonitor  
- IndividualRecognition  
- SoundRecorder  
- VoiceRecognition  
  - AutoDisable/DataCount/DataEventEnabledプロパティ, ClearInput/ClearInputProperiesメソッド, DataEvent/ErrorEventイベントをサポートしていること  
  - このプロジェクトでは RFIDScanner として実装  
  - 他には ElectronicValueRW が使える  

- GestureControl  
- GraphicDisplay  
- SoundPlayer  
- SpeechSynthesis  
  - OutputIDプロパティ, ClearOutputメソッド, ErrorEvent/OutputCompleteEventイベントをサポートしていること  
  - このプロジェクトでは GestureControl,GraphicDisplay を RemoteOrderDisplay として、SoundPlayer,SpeechSynthesis を ToneIndicator として実装  
  - 他には CAT, FiscalPrinter, POSPrinter, BumpBar, PointCardRW, SmartCardRW, ElectronicJournal, RFIDScanner, ElectronicValueRW が使える  


## 設定

POS for.NETのposdm.exeプログラムを使用してデバイスエントリ作成およびプロパティ設定を行ってください。  

- posdmのADDDEVICEコマンドでデバイスエントリを作成  
  使用例: posdm ADDDEVICE OposDmon1 /type:RFIDScanner /soname:"OpenPOS 1.16 DeviceMonitor"  

  - 指定したデバイス名(例では"OposDmon1")は"HardwarePath"の値として格納されます  
    他のPOS for.NETの名前やOPOSの名前と重ならないユニークな名前を指定してください  
  - /soname:には頭に"OpenPOS 1.16 "を付けてダブルクォーテーションで囲んだデバイスクラス名を指定してください  
    例えば "OpenPOS 1.16 CashDrawer", "OpenPOS 1.16 POSPrinter", "OpenPOS 1.16 VideoCapture", "OpenPOS 1.16 GestureControl" 等  

- posdmのADDPROPERTYコマンドで使用するOPOSデバイス名を設定  
  使用例: posdm ADDPROPERTY OposDeviceName VenderName_ModelName /type:RFIDScanner /soname:"OpenPOS 1.16 DeviceMonitor" /path:OposDmon1  

  - 設定対象のプロパティ名は "OposDeviceName"  
  - 設定する値(例では"VenderName_ModelName")はOPOSレジストリに存在するデバイスネームキーまたは論理デバイス名を指定してください  

上記使用例実行後のConfiguration.xmlの対象デバイスエントリ

    <ServiceObject Name="OpenPOS 1.16 DeviceMonitor" Type="RFIDScanner">  
      <Device HardwarePath="" Enabled="yes" PnP="no" />  
      <Device HardwarePath="OposDmon1" Enabled="yes" PnP="no">  
        <Property Name="OposDeviceName" Value="VenderName_ModelName" />  
      </Device>  
    </ServiceObject>  

## 呼び出し方

上記設定の使用例で作成したデバイスエントリを呼び出す手順と例を示します。

- プロジェクトの参照に"OpenPOS.Extension.Ver115"を追加する(以下にある)
  "C:\\Windows\\Microsoft.NET\\assembly\\GAC_MSIL\\OpenPOS.Extension.Ver115\\v4.0_1.15.0.0__ad2c9a67c3439201\\OpenPOS.Extension.Ver115.dll"
- プロジェクトの参照に"OpenPOS.Extension.Ver116"を追加する(以下にある)
  "C:\\Windows\\Microsoft.NET\\assembly\\GAC_MSIL\\OpenPOS.Extension.Ver116\\v4.0_1.16.0.0__ad2c9a67c3439201\\OpenPOS.Extension.Ver116.dll"
- ソースコードに"using OpenPOS.Extension;"を追加する
- PosExplorerのGetDevicesメソッドにデバイスクラス名やDeviceCompatibilitiesを指定して呼び出し、該当デバイスクラスのデバイスコレクションを取得  
- 取得したデバイスコレクションの中で ServiceObjectName と HardwarePath が一致するDeviceInfoを検索し、それを基にCreateInstanceメソッドでオブジェクトを生成  
- イベントハンドラを登録  
- Openメソッドを呼び出す  

コード例:


    RFIDScanner dmonObj1 = null;
    PosExplorer explorer = new PosExplorer();
    DeviceCollection rfidList = explorer.GetDevices("RFIDScanner", DeviceCompatibilities.CompatibilityLevel1);
    foreach (DeviceInfo rfidInfo in rfidList)
    {
        if  (rfidInfo.ServiceObjectName == "OpenPOS 1.16 DeviceMonitor")
        {
            if (rfidInfo.HardwarePath == "OposDmon1")
            {
                dmonObj1 = (RFIDScanner)explorer.CreateInstance(rfidInfo);
                break;
            }
        }
    }
    if (dmonObj1 != null)
    {
        dmonObj1.DataEvent += dmonObj1_DataEvent;
        dmonObj1.DirestIOEvent += dmonObj1_DirectIOEvent;
        dmonObj1.ErrorEvent += dmonObj1_ErrorEvent;
        dmonObj1.StatusUpdateEvent += dmonObj1_StatusUpdateEvent;

        dmonObj1.Open();
    }


注) Compatibilityプロパティ(DeviceCompatibilities)の値は場合によって変わります。  
DeviceCollection/DeviceInfoにリストされた状態では"CompatibilityLevel1"、CreateInstanceで生成されたオブジェクトでは"Opos"となります。  

- OpenPOS.Extension.Ver116のインターフェースを含んでいるかを確認してから UnifoedPOS 1.16 のメソッドを呼び出す  

コード例:


    // RFIDScanner dmonObj1 で宣言されている
    
    try
    {
        if (dmonObj1 is IDeviceMonitor116)
        {
            string sDeviceID = "Device01";
            MonitoringModeType eMMode = MonitoringModeType.Update;
            int iBoundary = 180;
            int iSubBoundary = 0;
            int iIntervalTime = 500;
            try
            {
                ((IDeviceMonitor116) dmonObj1).AddMonitoringDevice(sDeviceID, eMMode, iBoundary, iSubBoundary, iIntervalTime);
            }
            catch(UPOSException ue)
            {
            }
        }
    }
    catch(Exception ae)
    {
    }


## 既知の課題   

既知の課題は以下になります。

- 実際のOPOSやデバイスを使った動作確認は行っていません。  
- 特に以下のプロパティ/パラメータ/戻り値の、string(OPOS)とBitmap等(POS for.NET)との変換は正しいかどうか不明です。  
  - BiometricsデバイスのBiometricsInformationRecord(BIR)関連プロパティ/パラメータ/戻り値  
  - BiometricsデバイスのRawSensorDataプロパティ  
  - CheckScannerデバイスのImageDataプロパティ  
  - ImageScannerデバイスのFrameDataプロパティ  
- 動作記録採取や障害調査用情報取得などの機能がありません。  

## POS for.NETの課題  

開発過程でPOS for.NETに以下の課題が判明しました。  

- POS for.NETで定義されているBiometricsのBIRに情報が不足している。  
  UnifiedPOS仕様書に記述されているBIR図の以下の情報が入っていない。  
  - Quality  
  - Product ID (Owner, Type)  
  - Subtype  
  - Index Flag  
  - Index(UUID)  
  - Digital Signature  
- 同じくBIRで、課題かどうかは不明だが、以下の状況になっている。  
  - Creation Date/Creation Timeが、装置が情報を読み取った日時ではなく、POS for.NETサービスオブジェクトの中でBIRオブジェクトを作成した際の日時になっている。  
    POS for.NET自身のサービスオブジェクトでは問題無いかもしれないが、OPOSに中継する処理の場合は情報が書き換わってしまう。

## カスタマイズ

もし特定ユーザー/ベンダー固有の処理等のためのカスタマイズを加えたい場合、それは自由に行ってください。  
ただしその場合は、このサービスオブジェクトと同時に並行して使用しても問題無いように、以下の情報をすべて変更して独立したファイルにしてください。  

- ファイル名: OpenPOS.CCO116Interop.dll  
- namespace: OpenPOS.CCO116Interop  
- GUID: [assembly: Guid("8fd7a631-348b-4f02-a0bd-eb918d9effd5")]  
- サービスオブジェクト名: [ServiceObject(DeviceType.Xxxx, "OpenPOS 1.16 Xxxx",  
- クラス名: public class OpenPOSXxxx :  

注) 上記のうち "Xxxx" はUnifiedPOS/POS for.NETのデバイスクラス名が入る  

カスタマイズを行いたいデバイスだけを抜き出して新しく作るのが作業量も削減出来て良いでしょう。  

なお、汎用性のある良い機能ならば、こちらにも提案してください。

## ライセンス

[zlib License](./LICENSE) の下でライセンスされています。
