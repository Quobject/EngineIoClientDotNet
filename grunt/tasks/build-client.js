module.exports = function (grunt) {

  grunt.registerTask('buildClient',
      'build cs modules', function () {
    var
      fs = require('fs'),
      S = require('string'),
      string = require('string-formatter'),
      os = grunt.config('os'),
      config = grunt.config('config'),
      tasks = [],
      csproj = os === 'win' ? __dirname + '/../../EngineIoClientDotNet/EngineIoClientDotNet.csproj' :
//        __dirname + '/../../EngineIoClientDotNet/EngineIoClientDotNet_Mono.csproj',
        __dirname + '/../../EngineIoClientDotNet/EngineIoClientDotNet.csproj',
      build,
      clean,
      template_file_content,
      configuration = grunt.config('msbuild_configuration');

    template_file_content = fs.readFileSync('./templates/AssemblyInfo.cs');
    template_file_content = S(template_file_content).replaceAll('@VERSION@', config.version).s;
    //grunt.log.writeln('template_file_content = "%s"', template_file_content);
    fs.writeFileSync( __dirname + '/../../EngineIoClientDotNet/Properties/AssemblyInfo.cs', template_file_content );


    grunt.log.writeln('csproj = "%s"', csproj);


    if (os === 'win') {

      clean = '{0} start-process ' +
        '-NoNewWindow ' + 
        //'-WindowStyle Normal ' + //-WindowStyle (Hidden | Normal) | -NoNewWindow
        '-FilePath {1} ' +
        '-ArgumentList \' {2} /t:clean  /p:Configuration={3} \' ';
      clean = string.format(clean, config.win.powershell, config.win.msbuild, csproj, configuration );


      build = '{0} start-process ' +
        '-NoNewWindow ' + 
        //'-WindowStyle Normal ' + //-WindowStyle (Hidden | Normal) | -NoNewWindow
        '-FilePath {1} ' +
        '-ArgumentList \' {2}  /p:Configuration={3} \' ';
      build = string.format(build, config.win.powershell, config.win.msbuild, csproj, configuration );
    } else {
      clean = string.format('{0} {1} /t:clean /p:Configuration={2}', config.linux.msbuild,csproj, configuration);
      build = string.format('{0} {1} /p:Configuration={2}', config.linux.msbuild,csproj, configuration);
    }

    tasks.push(clean);
    tasks.push(build);

    grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
    grunt.config('shell.exec.command', tasks.join('&&'));
    grunt.task.run('shell');

  });
};


