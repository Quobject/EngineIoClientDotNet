module.exports = function (grunt) {

  grunt.registerTask('createNugetPackage',
     'create package ', function () {
    var
      fs = require('fs'),
      S = require('string'),
      string = require('string-formatter'),
      os = grunt.config('os'),
      config = grunt.config('config'),
      working_path = grunt.config('working_path'),
      //configuration = grunt.config('msbuild_configuration'),
      nuget_path = os === 'win' ?
        config.win.nuget : config.linux.nuget,
      tasks = [],
      template_file_content;

    //function createPackageWithTitle(title) {
    //  var
    //    dir_path = string.format('{0}/../../{1}/', __dirname, title),
    //    csproj = string.format('{0}{1}.csproj', dir_path, title),
    //    pack = string.format('{0} pack {1}', nuget_path, csproj);

    //  tasks.push(pack);
    //}

    if (os !== 'win') {
      return;
    } 

    //createPackageWithTitle('EngineIoClientDotNet');

       // 
    if (! fs.existsSync(working_path)) {
      fs.mkdirSync(working_path);
    }

    template_file_content = fs.readFileSync('./templates/EngineIoClientDotNet.nuspec');
    template_file_content = S(template_file_content).replaceAll('@VERSION@', config.version).s;

    fs.writeFileSync(string.format('{0}EngineIoClientDotNet.nuspec', working_path), template_file_content);
    //grunt.log.writeln('tasks = %s', JSON.stringify(tasks));
    //grunt.config('shell.exec.command', tasks.join('&&'));
    //grunt.task.run('shell');       
  });
};