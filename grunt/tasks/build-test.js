module.exports = function (grunt) {

  grunt.registerTask('buildTest',
      'build cs test modules', function () {
    var
      os = grunt.config('os'),
      string = require('string-formatter'),
      config = grunt.config('config'),
      tasks = [],
      csproj = os === 'win' ? __dirname + '/../../EngineIoClientDotNet_Tests/EngineIoClientDotNet_Tests.csproj':
        __dirname + '/../../EngineIoClientDotNet_Tests/EngineIoClientDotNet_Tests_Mono.csproj',
      build,
      clean;

    grunt.log.writeln('csproj = "%s"', csproj);


    if (os === 'win') {

      clean = '{0} start-process ' +
        '-NoNewWindow ' + 
        //'-WindowStyle Normal ' + //-WindowStyle (Hidden | Normal) | -NoNewWindow
        '-FilePath {1} ' +
        '-ArgumentList \' {2} /t:clean  /p:Configuration=Release \' ';
      clean = string.format(clean, config.win.powershell, config.win.msbuild, csproj );


      build = '{0} start-process ' +
        '-NoNewWindow ' + 
        //'-WindowStyle Normal ' + //-WindowStyle (Hidden | Normal) | -NoNewWindow
        '-FilePath {1} ' +
        '-ArgumentList \' {2}  /p:Configuration=Release \' ';
      build = string.format(build, config.win.powershell, config.win.msbuild, csproj );
    } else {
      clean = string.format('{0} {1} /t:clean /p:Configuration=Release', config.linux.msbuild,csproj);
      build = string.format('{0} {1} /p:Configuration=Release', config.linux.msbuild,csproj);
    }

    tasks.push(clean);
    tasks.push(build);

    grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
    grunt.config('shell.exec.command', tasks.join('&&'));
    grunt.task.run('shell');

  });
};


