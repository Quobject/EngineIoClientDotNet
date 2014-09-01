module.exports = function (grunt) {

  grunt.registerTask('createNugetPackage',
      'create package ', function () {
    var
      string = require('string-formatter'),
      os = grunt.config('os'),
      config = grunt.config('config'),
      tasks = [],
      nuget_path = os === 'win' ?
        config.win.nuget : config.linux.nuget,    
      project_dir = __dirname + '/../../EngineIoClientDotNet/',
      project = __dirname + '/../../EngineIoClientDotNet/EngineIoClientDotNet.csproj';

    if (os !== 'win') {
      return;
    }

    grunt.log.writeln('nuget_path = "%s"', nuget_path);

    tasks.push(  string.format('{0} pack {1}',nuget_path, project) );

    grunt.log.writeln('tasks = %s', JSON.stringify(tasks));

    grunt.config('shell.exec.options.execOptions.cwd', '<%= server_path %>');
    grunt.config('shell.exec.command', tasks.join('&&'));
    grunt.task.run('shell');

  });
};


