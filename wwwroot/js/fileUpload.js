document.addEventListener("DOMContentLoaded", function () {
    let selectedFiles = [];

    const fileInput = document.getElementById("files");
    const fileLabelText = document.getElementById("file-label-text");
    const fileList = document.getElementById("file-list");

    if (!fileInput || !fileLabelText || !fileList) {
        console.error("File upload elements not found!");
        return;
    }

    fileInput.addEventListener("change", function (event) {
        fileList.innerHTML = ""; // Clear previous list
        selectedFiles = Array.from(event.target.files); // Store files in array

        fileLabelText.textContent = selectedFiles.length > 0
            ? `${selectedFiles.length} file(s) selected`
            : "Click to upload files";

        selectedFiles.forEach((file, index) => {
            const listItem = document.createElement("li");
            listItem.innerHTML = `📄 ${file.name} <button onclick="removeFile(${index})">❌</button>`;
            fileList.appendChild(listItem);
        });
    });

    window.removeFile = function (index) {
        selectedFiles.splice(index, 1); // Remove file from array
        updateFileList();
    };

    function updateFileList() {
        fileList.innerHTML = "";

        fileLabelText.textContent = selectedFiles.length > 0
            ? `${selectedFiles.length} file(s) selected`
            : "Click to upload files";

        selectedFiles.forEach((file, index) => {
            const listItem = document.createElement("li");
            listItem.innerHTML = `📄 ${file.name} <button onclick="removeFile(${index})">❌</button>`;
            fileList.appendChild(listItem);
        });

        // Create a new FileList object and update the input field
        const dataTransfer = new DataTransfer();
        selectedFiles.forEach(file => dataTransfer.items.add(file));
        fileInput.files = dataTransfer.files;
    }
});
