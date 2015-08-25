/*!
 * FileInput Danish Translations
 *
 * This file must be loaded after 'fileinput.js'. Patterns in braces '{}', or
 * any HTML markup tags in the messages must not be converted or translated.
 *
 * @see http://github.com/kartik-v/bootstrap-fileinput
 *
 * NOTE: this file must be saved in UTF-8 encoding.
 */
(function ($) {
    "use strict";

    $.fn.fileinputLocales['da'] = {
        fileSingle: 'fil',
        filePlural: 'filer',
        browseLabel: 'Gennemse &hellip;',
        removeLabel: 'Fjern',
        removeTitle: 'Ryd valgte filer',
        cancelLabel: 'Anullér',
        cancelTitle: 'Afbryd',
        uploadLabel: 'Upload',
        uploadTitle: 'Upload valgte filer',
        msgSizeTooLarge: 'Filen "{name}" (<b>{size} KB</b>) overskrider den maksimale tilladte størrelse på <b>{maxSize} KB</b>. Prøv venligst igen efter at have gjort filen mindre',
        msgFilesTooLess: 'Du skal vælge mindst <b>{n}</b> {files} til upload. Prøv venligst igen',
        msgFilesTooMany: 'Antallet af filer <b>({n})</b> overskrider <b>{m}</b>. Reducér antallet af filer og prøv igen',
        msgFileNotFound: 'Filen "{name}" kan ikke findes',
        msgFileSecured: 'Du har ikke tilladelse til at læse filen "{name}".',
        msgFileNotReadable: 'Filen "{name}" kan ikke læses.',
        msgFilePreviewAborted: 'Preview annulleret "{name}".',
        msgFilePreviewError: 'Der opstod en fejl under læsning af filen "{name}".',
        msgInvalidFileType: 'Ugyldig filtype "{name}". Kun "{types}" er understøttet.',
        msgInvalidFileExtension: 'Ugyldig fil-endelse "{name}". Kun "{extensions}" er understøttet.',
        msgValidationError: 'Der opstod en fejl',
        msgLoading: 'Loader fil {index} af {files} &hellip;',
        msgProgress: 'Loader fil {index} af {files} - {name} - {percent}% færdig.',
        msgSelected: '{n} {files} valgt',
        msgFoldersNotAllowed: 'Kun drag & drop. Skipped {n} dropped folder(s).',
        dropZoneTitle: 'Drag & drop filer her &hellip;'
    };
})(window.jQuery);