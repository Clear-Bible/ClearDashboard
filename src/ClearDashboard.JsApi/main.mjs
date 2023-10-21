// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

import { dotnet } from './dotnet.js'

const { setModuleImports, getAssemblyExports, getConfig } = await dotnet
    .withDiagnosticTracing(false)
    .create();

setModuleImports('main.mjs', {
    node: {
        process: {
            version: () => globalThis.process.version
        }
    }
});

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

exports.Program.GetCorpusId('A4E23AAF-281D-42ED-9E1B-1468A1C73687').then((serializedCorpusId) => {
    let corpusId = JSON.parse(serializedCorpusId);
    console.log('Corpus id: ', corpusId)
});

await dotnet.run();
