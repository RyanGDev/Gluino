#include "filedialog.h"

#include <shobjidl_core.h>
#include <wrl/client.h>

using namespace Gluino;

using Microsoft::WRL::ComPtr;

template<typename T>
T* Create(HRESULT* hResult, cstr title, const cstr defaultPath) {
    static_assert(std::is_base_of_v<IFileDialog, T>, "T must inherit from IFileDialog");
    T* pfd = nullptr;
    const CLSID clsid =
        typeid(T) == typeid(IFileOpenDialog) ? CLSID_FileOpenDialog
        : typeid(T) == typeid(IFileSaveDialog) ? CLSID_FileSaveDialog
        : CLSID_FileOpenDialog;
    if (auto hr = CoCreateInstance(clsid, nullptr, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&pfd)); SUCCEEDED(hr)) {
        pfd->SetTitle(title);

        if (defaultPath) {
            IShellItem* psiDefault = nullptr;
            hr = SHCreateItemFromParsingName(defaultPath, nullptr, IID_PPV_ARGS(&psiDefault));
            if (SUCCEEDED(hr)) {
                pfd->SetFolder(psiDefault);
                psiDefault->Release();
            }
        }

        *hResult = hr;
        return pfd;
    }
    return nullptr;
}

HRESULT AddFilters(IFileDialog* pfd, const FileDialogFilter* filters, const int count) {
    if (count == 0) return S_OK;

    std::vector<COMDLG_FILTERSPEC> specs;
    for (int i = 0; i < count; i++) {
        COMDLG_FILTERSPEC spec;
        spec.pszName = filters[i].Name;
        spec.pszSpec = filters[i].Spec;
        specs.push_back(spec);
    }

    return pfd->SetFileTypes(count, specs.data());
}

HRESULT GetFileNames(IFileOpenDialog* pfd, cstr*& fileNames, int& fileNameCount) {
    IShellItemArray* psia = nullptr;
    HRESULT hr = pfd->GetResults(&psia);
    if (FAILED(hr)) return hr;

    if (psia == nullptr) {
        return E_POINTER;
    }

    DWORD count = 0;
    hr = psia->GetCount(&count);
    if (FAILED(hr)) {
        psia->Release();
        return hr;
    }

    fileNames = new cstr[count];
    for (DWORD i = 0; i < count; i++) {
        IShellItem* psi;
        hr = psia->GetItemAt(i, &psi);
        if (SUCCEEDED(hr)) {
            PWSTR name;
            hr = psi->GetDisplayName(SIGDN_FILESYSPATH, &name);
            if (SUCCEEDED(hr)) {
                fileNames[i] = _wcsdup(name);
                CoTaskMemFree(name);
            }
            psi->Release();
        }
        if (FAILED(hr)) {
            delete[] fileNames;
            psia->Release();
            return hr;
        }
    }

    fileNameCount = static_cast<int>(count);
    psia->Release();
    return S_OK;
}

DialogResult FileDialog::ShowDialog(const nwnd owner, FileDialogInfo* info) {

    HRESULT hr;
    ComPtr<IFileDialog> pfd;

    if (_type == FileDialogType::OpenFile || _type == FileDialogType::OpenFolder) {
        pfd = Create<IFileOpenDialog>(&hr, info->Title, info->DefaultPath);
    }
    else {
        pfd = Create<IFileSaveDialog>(&hr, info->Title, info->DefaultPath);
    }

    if (FAILED(hr)) return DialogResult::Error;

    DWORD dwOptions;
    hr = pfd->GetOptions(&dwOptions);
    if (FAILED(hr)) return DialogResult::Error;

    switch (_type) {
        case FileDialogType::OpenFile:
            hr = AddFilters(pfd.Get(), info->Filters, info->FilterCount);
            if (FAILED(hr)) return DialogResult::Error;
            dwOptions |= FOS_FILEMUSTEXIST | FOS_NOCHANGEDIR;
            if (info->MultiSelect) dwOptions |= FOS_ALLOWMULTISELECT;
            else                   dwOptions &= ~FOS_ALLOWMULTISELECT;
            break;
        case FileDialogType::OpenFolder:
            dwOptions |= FOS_PICKFOLDERS | FOS_NOCHANGEDIR;
            if (info->MultiSelect) dwOptions |= FOS_ALLOWMULTISELECT;
            else                   dwOptions &= ~FOS_ALLOWMULTISELECT;
            break;
        case FileDialogType::SaveFile:
            hr = AddFilters(pfd.Get(), info->Filters, info->FilterCount);
            if (FAILED(hr)) return DialogResult::Error;
            dwOptions |= FOS_NOCHANGEDIR;
            break;
    }

    hr = pfd->SetOptions(dwOptions);
    if (FAILED(hr)) return DialogResult::Error;

    hr = pfd->Show(owner);
    if (hr == HRESULT_FROM_WIN32(ERROR_CANCELLED)) return DialogResult::Cancel;
    if (FAILED(hr)) return DialogResult::Error;

    switch (_type) {
        case FileDialogType::OpenFile:
        case FileDialogType::OpenFolder: {
            hr = GetFileNames((IFileOpenDialog*)pfd.Get(), info->FileNames, info->FileNameCount);
            if (FAILED(hr)) return DialogResult::Error;
            break;
        }
        case FileDialogType::SaveFile: {
            IShellItem* psi;
            hr = pfd->GetResult(&psi);
            if (FAILED(hr)) return DialogResult::Error;
            PWSTR name;
            hr = psi->GetDisplayName(SIGDN_FILESYSPATH, &name);
            if (SUCCEEDED(hr)) {
                info->FileNames = new cstr[1];
                info->FileNames[0] = _wcsdup(name);
                info->FileNameCount = 1;
                CoTaskMemFree(name);
            }
            psi->Release();
            if (FAILED(hr)) return DialogResult::Error;
            break;
        }
    }

    return DialogResult::OK;
}
