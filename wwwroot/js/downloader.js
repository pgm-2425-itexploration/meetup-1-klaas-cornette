function downloadFile(filename, content) {
    const a = document.createElement("a");
    a.href = content;
    console.log(content);
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
}

async function getUploadedFileContent(fileInput) {
    const file = fileInput.files[0];
    if (!file) return null;

    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onload = () => resolve(reader.result);
        reader.onerror = reject;
        reader.readAsText(file);
    });
}
