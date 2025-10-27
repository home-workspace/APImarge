\
    # PdfMergeApi
    API en ASP.NET Core para unir múltiples PDFs y añadir imágenes en el orden indicado.

    ## Requisitos
    - .NET 8 SDK
    - Restaurar paquetes: `dotnet restore`

    ## Ejecutar
    `dotnet run --project PdfMergeApi.csproj`

    El endpoint principal: `POST /api/documents/merge` (multipart/form-data)
    - campo `files`: archivos a subir
    - campo `sequence`: JSON con la secuencia

    Ejemplo de `sequence.json`:
    ```json
    {
      "outputFileName": "final.pdf",
      "items": [
        { "fileName": "a.pdf", "type": "pdf" },
        { "fileName": "img1.png", "type": "image" },
        { "fileName": "b.pdf", "type": "pdf" }
      ]
    }
    ```

    ## Extensiones sugeridas
    - Overlay images on existing pages (extend SequenceItem with pageIndex and coordinates)
    - API Key authentication and rate limiting
    - Streaming for large files
