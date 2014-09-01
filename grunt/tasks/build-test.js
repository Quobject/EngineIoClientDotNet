module.exports = function (grunt) {

  grunt.registerTask('buildTest',
      'build cs test modules', function () {
    var
      fs = require('fs'),
      S = require('string'),
      string = require('string-formatter'),
      os = grunt.config('os'),
      config = grunt.config('config'),
      configuration = grunt.config('msbuild_configuration'),
      tasks = [],
      title = os === 'win' ? 'EngineIoClientDotNet_Tests' : 'EngineIoClientDotNet_Tests_Mono',
      dir_path = string.format('{0}/../../{1}/', __dirname, title),
      csproj = string.format('{0}{1}.csproj', dir_path, title),
      build_format,
      clean_format,
      build,
      clean;

    grunt.log.writeln('csproj = "%s"', csproj);


    if (os === 'win') {

      clean_format = '{0} start-process ' +
        '-NoNewWindow ' + 
        //'-WindowStyle Normal ' + //-WindowStyle (Hidden | Normal) | -NoNewWindow
        '-FilePath {1} ' +
        '-ArgumentList \' {2} /t:clean  /p:Configuration={3} \' ';
      clean = string.format(clean_format, config.win.powershell, config.win.msbuild, csproj, configuration );


      build_format = '{0} start-process ' +
        '-NoNewWindow ' + 
        //'-WindowStyle Normal ' + //-WindowStyle (Hidden | Normal) | -NoNewWindow
        '-FilePath {1} ' +
        '-ArgumentList \' {2}  /p:Configuration={3} \' ';
      build = string.format(build_format, config.win.powershell, config.win.msbuild, csproj, configuration );
    } else {
      clean = string.format('{0} {1} /t:clean /p:Configuration={2}', config.linux.msbuild, csproj, configuration);
      build = string.format('{0} {1} /p:Configuration={2}', config.linux.msbuild, csproj, configuration);
    }

    tasks.push(clean);
    tasks.push(build);

    grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
    grunt.config('shell.exec.command', tasks.join('&&'));
    grunt.task.run('shell');

  });
};


