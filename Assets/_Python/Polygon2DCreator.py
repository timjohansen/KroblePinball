import bpy
import bmesh

path = "/Users/timj/Documents/Unity Projects/Pinball/Assets/_ColliderData/"
fbxPath = "/Users/timj/Documents/Unity Projects/Pinball/Assets/_Models/"
obj_name = bpy.context.object.name
export_fbx = True
if "_COL" in obj_name:
    obj_name = obj_name[:len(obj_name) - 4]
    export_fbx = False
filename = obj_name + ".txt"
fbxFilename = obj_name + ".fbx"

if export_fbx:
    bpy.ops.export_scene.fbx(filepath=fbxPath + fbxFilename, use_selection=True, apply_unit_scale=True, apply_scale_options='FBX_SCALE_ALL', axis_forward='Y', axis_up='Z')

col_edges = []
used_edges = []
for edge in bpy.context.object.data.attributes['collision'].data:
    col_edges.append(edge)

sanity_timer = 0
first = True

bm = bmesh.new()  # create an empty BMesh
bm.from_mesh(bpy.context.object.data)  # fill it in from selected Mesh
bm.edges.ensure_lookup_table()

with open(path + filename, 'w') as file:
    while len(used_edges) < len(col_edges):
        #if not first:
        #    file.write("end")
        
        sanity_timer += 1
        if sanity_timer == 10000:
            raise Exception("ERROR: caught in an infinite loop")

        start_edge_ind = None
        for i in range(len(col_edges)):
            if col_edges[i].value and bm.edges[i] not in used_edges:
                start_edge_ind = i                
                break

        if start_edge_ind == None:
            break
        if not first:
            file.write("\n")
        first = False

        ordered_verts = []
        cur_edge = bm.edges[start_edge_ind]
        used_edges.append(cur_edge)
        prev_edge = None
        first_vert = cur_edge.verts[0]
        prev_vert = cur_edge.verts[0]
        cur_vert = cur_edge.verts[1]

        ordered_verts.append(cur_edge.verts[0])

        for linked_edge in cur_edge.verts[0].link_edges:
            if col_edges[linked_edge.index].value:  # the connecting edge is a collision edge
                if prev_edge is None or linked_edge != prev_edge:  # and it's not where we came from

                    prev_edge = cur_edge
                    cur_edge = linked_edge
                    used_edges.append(cur_edge)
                    cur_vert = linked_edge.other_vert(cur_edge.verts[0])
                    break

        if first_vert is None:
            raise Exception('No initial connecting collision edge could be found')

        while cur_vert != first_vert:
            ordered_verts.append(cur_vert)

            next_edge_found = False
            for linked_edge in cur_vert.link_edges:
                if linked_edge == cur_edge:
                    continue
                if col_edges[linked_edge.index].value == True:
                # prev_edge = cur_edge
                    cur_edge = linked_edge
                    used_edges.append(cur_edge)
                    next_edge_found = True
                    break

            next_vert = cur_edge.other_vert(cur_vert)

            if not next_edge_found:
                raise Exception('No continuous loop of collision edges could be found')
            cur_vert = next_vert


        for i in range(len(ordered_verts)):
            v = ordered_verts[i]
            file.write(str(v.co.x) + ',' + str(v.co.y) + ',' + str(v.co.z) + '\n')
        file.write("end")